import { navigateTo } from '@devvit/web/client';

import { showToast } from '@devvit/web/client';
console.log('[CLIENT] game entrypoint loaded');

window.navigateToPost = (url) => {
  console.log('[CLIENT] navigating to post:', url);

  showToast({
    text: 'Post created successfully 🎉',
    appearance: 'success', // 'neutral' | 'success'
  });

  // ⏳ wait before navigating
  setTimeout(() => {
    console.log('[CLIENT] navigating now:', url);
    navigateTo(url);
  }, 1200); // 1.2 seconds is a sweet spot
};

  


