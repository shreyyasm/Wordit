import { Hono } from 'hono';
import { context, redis, reddit } from '@devvit/web/server';
import type {
  InitResponse,
  LevelCompletedRequest,
  LevelCompletedResponse,
  SaveScoreRequest,
  SaveScoreResponse,
  SaveGlobalScoreRequest,
  SaveGlobalScoreResponse,
} from '../../shared/api';

type ErrorResponse = {
  status: 'error';
  message: string;
};

const LEADERBOARD_KEY = 'leaderboard:wordittt';

export const api = new Hono();

api.get('/init', async (c) => {
  const { postId } = context;

  if (!postId) {
    console.error('API Init Error: postId not found in devvit context');
    return c.json<ErrorResponse>(
      {
        status: 'error',
        message: 'postId is required but missing from context',
      },
      400
    );
  }

  try {
    const username = await reddit.getCurrentUsername();
    const currentUsername = username ?? 'anonymous';

    let snoovatarUrl = '';
    if (username && context.userId) {
      const user = await reddit.getUserById(context.userId);
      if (user) {
        snoovatarUrl = (await user.getSnoovatarUrl()) ?? '';
      }
    }

    const redisKey = `${postId}:${currentUsername}`;
    const previousTime = await redis.get(redisKey);

    // Read user's score from the leaderboard sorted set
    const rawScore = await redis.zScore(LEADERBOARD_KEY, currentUsername);
    const score = typeof rawScore === 'number' ? rawScore : Number(rawScore ?? 0);

    // Read global score
    const globalscore = await redis.get('leaderboard:global');

    return c.json<InitResponse>({
      type: 'init',
      postId: postId,
      username: currentUsername,
      snoovatarUrl: snoovatarUrl,
      previousTime: previousTime ?? '',
      score: score ?? 0,
      globalscore: globalscore ?? '',
    });
  } catch (error) {
    console.error(`API Init Error for post ${postId}:`, error);
    let errorMessage = 'Unknown error during initialization';
    if (error instanceof Error) {
      errorMessage = `Initialization failed: ${error.message}`;
    }
    return c.json<ErrorResponse>(
      { status: 'error', message: errorMessage },
      400
    );
  }
});

api.post('/level-completed', async (c) => {
  const { postId } = context;

  if (!postId) {
    console.error('No postId in context');
    return c.json<ErrorResponse>(
      {
        status: 'error',
        message: 'postId is required',
      },
      400
    );
  }

  let body: LevelCompletedRequest;
  try {
    body = await c.req.json<LevelCompletedRequest>();
  } catch (error) {
    console.error('Invalid JSON body for level-completed', error);
    return c.json<ErrorResponse>(
      {
        status: 'error',
        message: 'Invalid JSON body',
      },
      400
    );
  }

  try {
    const { username, time } = body;

    if (!username || !time) {
      console.error('Missing username or time in request');
      return c.json<ErrorResponse>(
        {
          status: 'error',
          message: 'username and time are required',
        },
        400
      );
    }

    const redisKey = `${postId}:${username}`;
    await redis.set(redisKey, time);

    return c.json<LevelCompletedResponse>({
      type: 'level-completed',
      success: true,
      message: 'Time saved successfully',
    });
  } catch (error) {
    console.error(`API Level Completed Error for post ${postId}:`, error);
    let errorMessage = 'Unknown error saving completion time';
    if (error instanceof Error) {
      errorMessage = `Failed to save time: ${error.message}`;
    }
    return c.json<LevelCompletedResponse>(
      {
        type: 'level-completed',
        success: false,
        message: errorMessage,
      },
      500
    );
  }
});

// Save a user's score to the leaderboard (only if higher than existing)
api.post('/save-score', async (c) => {
  try {
    const body = await c.req.json<SaveScoreRequest>();
    const { score } = body;

    const username = await reddit.getCurrentUsername();

    if (!username || typeof score !== 'number') {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'valid username and numeric score required' },
        400
      );
    }

    const existingRaw = await redis.zScore(LEADERBOARD_KEY, username);
    const existingScore = typeof existingRaw === 'number' ? existingRaw : Number(existingRaw ?? 0);

    if (score > existingScore) {
      await redis.zAdd(LEADERBOARD_KEY, { member: username, score });
    }

    return c.json<SaveScoreResponse>({
      type: 'save-score',
      success: true,
      message: 'score saved',
    });
  } catch (error) {
    console.error('[DEVVIT] save-score failed:', error);
    return c.json<SaveScoreResponse>(
      { type: 'save-score', success: false, message: 'failed to save score' },
      500
    );
  }
});

import { reddit } from '@devvit/web/server'

api.post('/custom-post-create', async (c) => {
  try {
    const body = await c.req.json<{ subredditName: string }>()

    if (!body?.subredditName) {
      return c.json(
        { status: 'error', message: 'subredditName required' },
        400
      )
    }

    const post = await reddit.submitCustomPost({
      runAs: 'User', // must be 'User' for submitCustomPost
      subredditName: body.subredditName,
      title: 'Adventure Game',
      entry: 'PostCreated', // ✅ MUST match devvit.json entry key
      userGeneratedContent: {
        text: "Hello there! This is a new post from the user's account",
      },
      postData: {
        gameState: 'active',
        initialized: true,
      },
    })

    return c.json({
      status: 'success',
      postUrl: `https://reddit.com/r/${body.subredditName}/comments/${post.id}`,
      postId: post.id,
    })
  } catch (err) {
    console.error('[CUSTOM POST CREATE]', err)
    return c.json(
      { status: 'error', message: 'failed-to-create-post' },
      500
    )
  }
})
// Save a global score value
api.post('/save-global-score', async (c) => {
  const { postId } = context;

  if (!postId) {
    return c.json<ErrorResponse>(
      { status: 'error', message: 'postId is required' },
      400
    );
  }

  try {
    const body = await c.req.json<SaveGlobalScoreRequest>();
    const { username, globalscore } = body;

    if (!username || !globalscore) {
      return c.json<ErrorResponse>(
        { status: 'error', message: 'username and globalscore are required' },
        400
      );
    }

    await redis.set('leaderboard:global', globalscore);

    return c.json<SaveGlobalScoreResponse>({
      type: 'save-global-score',
      success: true,
      message: 'globalscore saved successfully',
    });
  } catch (error) {
    console.error(`API globalscore Save Error for post ${postId}:`, error);
    let errorMessage = 'Unknown error saving globalscore';
    if (error instanceof Error) {
      errorMessage = `Failed to save globalscore: ${error.message}`;
    }
    return c.json<SaveGlobalScoreResponse>(
      { type: 'save-global-score', success: false, message: errorMessage },
      500
    );
  }
});

// Get leaderboard (top 10)
api.get('/leaderboard', async (c) => {
  try {
    const members = await redis.zRange(LEADERBOARD_KEY, 0, 9);

    if (!members || members.length === 0) {
      return c.json({ leaderboard: [] });
    }

    const leaderboard = members.map((entry, i) => ({
      rank: i + 1,
      username: entry.member,
      score: entry.score,
    }));

    return c.json({ leaderboard });
  } catch (err) {
    console.error('[DEVVIT] leaderboard failed:', err);
    return c.json({ error: 'failed-to-load' }, 500);
  }
});

// Get current user's rank
api.get('/user-rank', async (c) => {
  try {
    const username = await reddit.getCurrentUsername();

    if (!username) {
      return c.json({ rank: null });
    }

    const rawRank = await redis.zRank(LEADERBOARD_KEY, username, { reverse: true });

    if (rawRank === null) {
      return c.json({ rank: null });
    }

    const rawScore = await redis.zScore(LEADERBOARD_KEY, username);
    const score = typeof rawScore === 'number' ? rawScore : Number(rawScore ?? 0);

    return c.json({
      rank: rawRank + 1,
      username,
      score,
    });
  } catch (err) {
    console.error('[DEVVIT] User rank fetch failed', err);
    return c.json({ error: 'failed-to-load-user-rank' }, 500);
  }
});