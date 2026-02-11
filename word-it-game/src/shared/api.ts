export type InitResponse = {
  type: 'init';
  postId: string;
  username: string;
  snoovatarUrl: string;
  previousTime: string; // empty string if no previous time exists
  score: number;
  globalscore: string;
};

export type LevelCompletedRequest = {
  type: 'level-completed';
  username: string;
  postId: string;
  time: string;
};

export type LevelCompletedResponse = {
  type: 'level-completed';
  success: boolean;
  message?: string;
};

export type SaveScoreRequest = {
  type: 'save-score';
  score: number;
};

export type SaveScoreResponse = {
  type: 'save-score';
  success: boolean;
  message?: string;
};

export type SaveGlobalScoreRequest = {
  type: 'save-global-score';
  username: string;
  globalscore: string;
};

export type SaveGlobalScoreResponse = {
  type: 'save-global-score';
  success: boolean;
  message?: string;
};