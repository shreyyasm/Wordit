

import { requestExpandedMode } from '@devvit/web/client';

document.addEventListener('DOMContentLoaded', () => {
  const playButton = document.getElementById('play-button');

  playButton.addEventListener('click', async (event) => {
    try {
      await requestExpandedMode(event, 'game');
    } catch (error) {
      console.error('Failed to enter expanded mode:', error);
    }
  });
});




