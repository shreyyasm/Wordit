export type InitResponse = {
  type: "init";
  postId: string;
  username: string;
  snoovatarUrl: string;
  previousTime: string; // empty string if no previous time exists
};

export type LevelCompletedRequest = {
  type: "level-completed";
  username: string;
  postId: string;
  time: string;
};

export type LevelCompletedResponse = {
  type: "level-completed";
  success: boolean;
  message?: string;
};

export type SaveScoreRequest = {
  type: "save-score";
  username: string;
  postId: string;
  time: string;
};

export type SaveScoreResponse = {
  type: "save-score";
  success: boolean;
  message?: string;
};

export type SaveGlobalScoreRequest = {
  type: "save-global-score";
  username: string;
  postId: string;
  time: string;
};

export type SaveGlobalScoreResponse = {
  type: "save-global-score";
  success: boolean;
  message?: string;
};
