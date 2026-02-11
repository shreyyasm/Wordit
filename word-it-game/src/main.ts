import { Devvit } from '@devvit/public-api'

Devvit.configure({
  redditAPI: true,
})

Devvit.addMenuItem({
  label: 'Create Game Post',
  location: 'subreddit',
  onPress: async (_, context) => {
    const subredditName = context.subredditName

    if (!subredditName) {
      throw new Error('subredditName missing from context')
    }

    const post = await context.reddit.submitCustomPost({
      subredditName,
      title: 'Adventure Game',
      entry: 'default', // MUST match devvit.json
      postData: {
        gameState: 'active',
        initialized: true,
      },
    })

    console.log('Post created:', post.id)
  },
})