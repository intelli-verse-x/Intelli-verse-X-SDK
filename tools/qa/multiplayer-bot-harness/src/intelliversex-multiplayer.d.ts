declare module "@intelliversex/multiplayer" {
  export const createClient: undefined | ((opts: { host: string }) => {
    connect(): Promise<void>;
    joinMatch(req: { matchId: string }): Promise<unknown>;
    createMatch(req: {
      templateId: string;
      gameId?: string;
      templateInit?: Record<string, unknown>;
    }): Promise<unknown>;
  });
}
