declare module "livekit-client" {
  export const Room: any;
  export const RoomEvent: any;
  export const ConnectionState: any;
  export function createLocalAudioTrack(...args: any[]): Promise<any>;
}
