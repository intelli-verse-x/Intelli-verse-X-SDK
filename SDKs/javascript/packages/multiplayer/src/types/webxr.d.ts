interface XRSpace {}

interface XRReferenceSpace extends XRSpace {}

interface XRViewerPose {
  transform: {
    position: { x: number; y: number; z: number };
    orientation: { x: number; y: number; z: number; w: number };
  };
}

interface XRPose {
  transform: {
    position: { x: number; y: number; z: number };
    orientation: { x: number; y: number; z: number; w: number };
  };
}

interface XRInputSource {
  gripSpace?: XRSpace;
  handedness?: "none" | "left" | "right";
}

interface XRFrame {
  getViewerPose(referenceSpace: XRReferenceSpace): XRViewerPose | null;
  getPose(space: XRSpace, baseSpace: XRReferenceSpace): XRPose | null;
}

interface XRSession extends EventTarget {
  readonly inputSources: Iterable<XRInputSource>;
  requestAnimationFrame(callback: (time: DOMHighResTimeStamp, frame: XRFrame) => void): number;
}
