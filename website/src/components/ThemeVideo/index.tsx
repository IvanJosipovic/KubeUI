import React, {useEffect, useRef} from 'react';
import {useColorMode} from '@docusaurus/theme-common';

type ThemeVideoProps = {
  name: string;
};

const VIDEO_TYPE = 'video/mp4; codecs="avc1.640028, mp4a.40.2"';

export default function ThemeVideo({name}: ThemeVideoProps): React.JSX.Element {
  const {colorMode} = useColorMode();
  const lightVideo = useRef<HTMLVideoElement>(null);
  const darkVideo = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (lightVideo.current) {
      lightVideo.current.preload = colorMode === 'light' ? 'auto' : 'none';
    }
    if (darkVideo.current) {
      darkVideo.current.preload = colorMode === 'dark' ? 'auto' : 'none';
    }
  }, [colorMode]);

  return (
    <>
      <video ref={lightVideo} className="theme-video theme-video--light" controls preload="none" width="100%">
        <source src={`/video/${name}-light.mp4`} type={VIDEO_TYPE} />
        Your browser does not support embedded video.
      </video>
      <video ref={darkVideo} className="theme-video theme-video--dark" controls preload="none" width="100%">
        <source src={`/video/${name}-dark.mp4`} type={VIDEO_TYPE} />
        Your browser does not support embedded video.
      </video>
    </>
  );
}
