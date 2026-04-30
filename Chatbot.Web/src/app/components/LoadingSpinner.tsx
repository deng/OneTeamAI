export function LoadingSpinner({ text = '加载中...' }: { text?: string }) {
  return <span className="spinner">{text}</span>;
}
