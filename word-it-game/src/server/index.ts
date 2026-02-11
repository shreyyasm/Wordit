import { Hono } from 'hono';
import { serve } from '@hono/node-server';
import { createServer, getServerPort } from '@devvit/web/server';
import { api } from './routes/api';
import { forms } from './routes/forms';
import { menu } from './routes/menu';
import { triggers } from './routes/triggers';

const app = new Hono();
const internal = new Hono();

internal.route('/menu', menu);
internal.route('/form', forms);
internal.route('/triggers', triggers);

app.route('/api', api);
app.route('/internal', internal);

serve({
  fetch: app.fetch,
  createServer,
  port: getServerPort(),
});


import {
  InitResponse,
  LevelCompletedRequest,
  LevelCompletedResponse,
} from "../shared/types/api";
import {
  createServer,
  context,
  getServerPort,
  reddit,
  redis,
} from "@devvit/web/server";
import { createPost } from "./core/post";

// Example to show how to send initial data to the Unity Game
app.get<
  { postId: string },
  InitResponse | { status: string; message: string }
>("/api/init", async (_req, res): Promise<void> => {
  const { postId } = context;

  if (!postId) {
    console.error("API Init Error: postId not found in devvit context");
    res.status(400).json({
      status: "error",
      message: "postId is required but missing from context",
    });
    return;
  }

  try {
    const username = await reddit.getCurrentUsername();
    const currentUsername = username ?? "anonymous";
    
    // Fetch user info for snoovatar
    let snoovatarUrl = "";
    if (username && context.userId) {
      const user = await reddit.getUserById(context.userId);
      if (user) {
        snoovatarUrl = (await user.getSnoovatarUrl()) ?? "";
      }
    }
    
    // Fetch previous time from Redis using postId:username as key
    const redisKey = `${postId}:${currentUsername}`;
    const previousTime = await redis.get(redisKey);

    // ✅ ZSET score read (NOT redis.get)
    const rawScore = await redis.zScore(
      'leaderboard:wordittt',
      currentUsername
    );
    const score =
      typeof rawScore === 'number'
        ? rawScore
        : Number(rawScore ?? 0);


     const redisKeyGlobalScore = `leaderboard:global`;
    const globalscore = await redis.get(redisKeyGlobalScore);

    res.json({
      type: "init",
      postId: postId,
      username: currentUsername,
      snoovatarUrl: snoovatarUrl,
      previousTime: previousTime ?? "",
      score: score ?? "",
      globalscore: globalscore ?? "",
    });
  } catch (error) {
    console.error(`API Init Error for post ${postId}:`, error);
    let errorMessage = "Unknown error during initialization";
    if (error instanceof Error) {
      errorMessage = `Initialization failed: ${error.message}`;
    }
    res.status(400).json({ status: "error", message: errorMessage });
  }
});

app.post<
  unknown,
  LevelCompletedResponse | { status: string; message: string },
  LevelCompletedRequest
>("/api/level-completed", async (req, res): Promise<void> => {
  const { postId } = context;
  
  if (!postId) {
    console.error("No postId in context");
    res.status(400).json({
      status: "error",
      message: "postId is required",
    });
    return;
  }

  try {
    const { username, time } = req.body;
    
    if (!username || !time) {
      console.error("Missing username or time in request");
      res.status(400).json({
        status: "error",
        message: "username and time are required",
      });
      return;
    }

    // Store the completion time in Redis with key format: postId:username
    const redisKey = `${postId}:${username}`;
    await redis.set(redisKey, time);

    res.json({
      type: "level-completed",
      success: true,
      message: "Time saved successfully",
    });
  } catch (error) {
    console.error(`API Level Completed Error for post ${postId}:`, error);
    let errorMessage = "Unknown error saving completion time";
    if (error instanceof Error) {
      errorMessage = `Failed to save time: ${error.message}`;
    }
    res.status(500).json({
      type: "level-completed",
      success: false,
      message: errorMessage,
    });
  }
});

app.post('/internal/on-app-install', async (_req, res): Promise<void> => {
  try {
    const post = await createPost();

    res.json({
      status: 'success',
      message: `Post created in subreddit ${context.subredditName} with id ${post.id}`,
    });
  } catch (error) {
    console.error(`Error creating post: ${error}`);
    res.status(400).json({
      status: 'error',
      message: 'Failed to create post',
    });
  }
});

app.post('/internal/menu/post-create', async (_req, res): Promise<void> => {
  try {
    const post = await createPost();
    post

    res.json({
      navigateTo: `https://reddit.com/r/${context.subredditName}/comments/${post.id}`,
    });
  } catch (error) {
    console.error(`Error creating post: ${error}`);
    res.status(400).json({
      status: 'error',
      message: 'Failed to create post',
    });
  }
});

//app.use(router);

const server = createServer(app);
server.on("error", (err) => console.error(`server error; ${err.stack}`));
server.listen(getServerPort());

import { reddit } from '@devvit/web/server';
app.post('/api/custom-post-create', async (_req, res) => {
  const { subredditName } = context;
  if (!subredditName) {
    res.status(400).json({ error: 'subredditName required' });
    return;
  }

  const post = await reddit.submitCustomPost({
      runAs: 'User',
      subredditName,
      userGeneratedContent: {
      text: "Hello there! This is a new post from the user's account",},
      title: 'Adventure Game',
      entry: 'PostCreated', // MUST match devvit.json entrypoint key
      postData: {
        gameState: 'active',
        initialized: true,
      },
    });

  res. json({
    success: true,
    postUrl: `https://reddit.com/r/${subredditName}/comments/${post.id}`
  });
});

import type { Devvit } from '@devvit/public-api'

import {
  SaveScoreRequest,
} from '../shared/types/api'

app.post('/api/save-score', async (c) => {
  try {
    const { reddit, redis } = c.env

    if (!reddit || !redis) {
      throw new Error('Devvit env not available')
    }

    const body = await c.req.json<SaveScoreRequest>()
    const { score } = body

    // ✅ CORRECT API FOR WEB SERVERS
    const me = await reddit.getMe()
    const username = me?.name

    if (!username || typeof score !== 'number') {
      return c.json(
        { status: 'error', message: 'valid score required' },
        400
      )
    }

    const LEADERBOARD_KEY = 'leaderboard:wordittt'

    const existingRaw = await redis.zScore(
      LEADERBOARD_KEY,
      username
    )

    const existingScore =
      typeof existingRaw === 'number'
        ? existingRaw
        : Number(existingRaw ?? 0)

    if (score > existingScore) {
      await redis.zAdd(LEADERBOARD_KEY, {
        member: username,
        score,
      })
    }

    return c.json({
      type: 'save-score',
      success: true,
      message: 'score saved',
    })
  } catch (error) {
    console.error('[DEVVIT] save-score failed:', error)

    return c.json(
      {
        type: 'save-score',
        success: false,
        message: 'failed to save score',
      },
      500
    )
  }
})


/**
 * ✅ THIS is what actually registers your server
 */
serve(app)


Devvit.addServer(app)
import {
  InitResponse,
  SaveGlobalScoreRequest,
  SaveGlobalScoreResponse,
} from "../shared/types/api";

app.post<
  unknown,
  SaveGlobalScoreResponse | { status: string; message: string },
  SaveGlobalScoreRequest
>("/api/save-global-score", async (req, res): Promise<void> => {
  const { postId } = context;
  
  if (!postId) {
    console.error("No postId in context");
    res.status(400).json({
      status: "error",
      message: "postId is required",
    });
    return;
  }

  try {
    const { username, globalscore } = req.body;
    
    if (!username || ! globalscore) {
      console.error("Missing username or  globalscore in request");
      res.status(400).json({
        status: "error",
        message: "username and  globalscore are required",
      });
      return;
    }

    // Store the completion  globalscore in Redis with key format: postId:username
    const redisKeyGlobalScore = `leaderboard:global`;
    await redis.set( redisKeyGlobalScore, globalscore);

    res.json({
      type: "save-global-score",
      success: true,
      message: " globalscore saved successfully",
    });
  } catch (error) {
    console.error(`API  globalscore Save Error for post ${postId}:`, error);
    let errorMessage = "Unknown error saving  globalscore";
    if (error instanceof Error) {
      errorMessage = `Failed to save  globalscore: ${error.message}`;
    }
    res.status(500).json({
      type: "save-global-score",
      success: false,
      message: errorMessage,
    });
  }
});

const LEADERBOARD_KEY = 'leaderboard:wordittt';
let leaderboardSanitized = false;

async function sanitizeLeaderboardOnce() {
  if (leaderboardSanitized) return;

  try {
    await redis.zRem(LEADERBOARD_KEY, '[object Object]');
    leaderboardSanitized = true;
    console.log('[DEVVIT] Leaderboard sanitized');
  } catch (err) {
    console.error('[DEVVIT] Leaderboard sanitize failed', err);
  }
}

app.get('/api/leaderboard', async (_req, res) => {
  try {
    const LEADERBOARD_KEY = 'leaderboard:wordittt';

    const members = await redis.zRange(LEADERBOARD_KEY, 0, 9);

    if (!members || members.length === 0) {
      res.json({ leaderboard: [] });
      return;
    }

    const leaderboard = [];

    for (let i = 0; i < members.length; i++) {
      leaderboard.push({
        rank: i + 1,
        username: members[i].member,
        score: members[i].score
      });
    }

    res.json({ leaderboard });
  } catch (err) {
    console.error('[DEVVIT] leaderboard failed:', err);
    res.status(500).json({ error: 'failed-to-load' });
  }
});


app.get('/api/user-rank', async (c) => {
  try {
    const { reddit, redis } = c.env
    const key = 'leaderboard:worditt'

    const username = await reddit.getCurrentUsername()

    if (!username) {
      return c.json({ rank: null })
    }

    const rawRank = await redis.zRank(
      key,
      username,
      { reverse: true }
    )

    // User not ranked yet OR leaderboard empty
    if (rawRank === null) {
      return c.json({ rank: null })
    }

    const rawScore = await redis.zScore(key, username)
    const score =
      typeof rawScore === 'number'
        ? rawScore
        : Number(rawScore ?? 0)

    return c.json({
      rank: rawRank + 1, // convert to 1-based
      username,
      score,
    })
  } catch (err) {
    console.error('[DEVVIT] User rank fetch failed', err)
    return c.json(
      { error: 'failed-to-load-user-rank' },
      500
    )
  }
})
