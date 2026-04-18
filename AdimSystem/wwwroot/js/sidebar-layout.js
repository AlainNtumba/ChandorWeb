export function subscribeNarrowViewport(dotNetHelper, breakpointPx) {
  const mql = window.matchMedia(`(max-width: ${breakpointPx}px)`);
  const listener = () => {
    dotNetHelper.invokeMethodAsync('OnNarrowViewport', mql.matches);
  };
  mql.addEventListener('change', listener);
  dotNetHelper.invokeMethodAsync('OnNarrowViewport', mql.matches);
  return {
    dispose: () => mql.removeEventListener('change', listener)
  };
}
