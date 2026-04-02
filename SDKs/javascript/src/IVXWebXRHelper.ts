// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

/**
 * WebXR session types corresponding to the immersive-vr, immersive-ar,
 * and inline modes defined by the WebXR Device API.
 */
export enum WebXRSessionType {
  ImmersiveVR = 'immersive-vr',
  ImmersiveAR = 'immersive-ar',
  Inline = 'inline',
}

/** Capability flags detected from the current browser / device. */
export interface WebXRCapabilities {
  vrSupported: boolean;
  arSupported: boolean;
  inlineSupported: boolean;
  deviceName?: string;
}

/**
 * Lightweight helper that wraps the WebXR Device API with
 * capability detection, session management, and lifecycle callbacks.
 */
export class IVXWebXRHelper {
  private static _instance: IVXWebXRHelper;
  private _activeSession: XRSession | null = null;

  /** Singleton accessor. */
  static getInstance(): IVXWebXRHelper {
    if (!IVXWebXRHelper._instance) {
      IVXWebXRHelper._instance = new IVXWebXRHelper();
    }
    return IVXWebXRHelper._instance;
  }

  /** Fired when an XR session starts. */
  onSessionStart?: (session: XRSession) => void;
  /** Fired when the active XR session ends. */
  onSessionEnd?: () => void;

  /** Returns  when the browser exposes the WebXR API. */
  isWebXRSupported(): boolean {
    return typeof navigator !== 'undefined' && 'xr' in navigator;
  }

  /**
   * Probes the browser for supported session types and returns a
   * {@link WebXRCapabilities} snapshot.
   */
  async detectCapabilities(): Promise<WebXRCapabilities> {
    const caps: WebXRCapabilities = {
      vrSupported: false,
      arSupported: false,
      inlineSupported: false,
    };

    if (!this.isWebXRSupported()) return caps;

    const xr = navigator.xr!;
    const [vr, ar, inline] = await Promise.all([
      xr.isSessionSupported('immersive-vr').catch(() => false),
      xr.isSessionSupported('immersive-ar').catch(() => false),
      xr.isSessionSupported('inline').catch(() => false),
    ]);

    caps.vrSupported = vr;
    caps.arSupported = ar;
    caps.inlineSupported = inline;

    return caps;
  }

  /**
   * Requests a WebXR session of the given type.
   *
   * @param type    - The {@link WebXRSessionType} to request.
   * @param options - Optional  features (required / optional).
   * @returns The  on success, or  if the request fails.
   */
  async requestSession(
    type: WebXRSessionType,
    options?: XRSessionInit,
  ): Promise<XRSession | null> {
    if (!this.isWebXRSupported()) {
      console.warn('[IVX] WebXR is not supported in this browser');
      return null;
    }

    try {
      const session = await navigator.xr!.requestSession(type, options);
      this._activeSession = session;

      session.addEventListener('end', () => {
        this._activeSession = null;
        this.onSessionEnd?.();
      });

      this.onSessionStart?.(session);
      return session;
    } catch (err) {
      console.error('[IVX] Failed to start XR session:', err);
      return null;
    }
  }

  /** Returns the currently active session, if any. */
  get activeSession(): XRSession | null {
    return this._activeSession;
  }

  /** Ends the active XR session gracefully. */
  async endSession(): Promise<void> {
    if (this._activeSession) {
      await this._activeSession.end();
      this._activeSession = null;
    }
  }
}
