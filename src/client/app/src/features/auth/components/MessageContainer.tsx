import clsx from "clsx";
import { CircleCheckBig, TriangleAlert } from "lucide-react";

interface MessageContainerProps {
  message: string;
  type: "success" | "error" | "info" | "warning";
}

function MessageContainer({ type, message }: MessageContainerProps) {
  const iconStyle = clsx({
    "text-blue-600": type === "info",
    "text-green-600": type === "success",
    "text-red-600": type === "error",
    "text-yellow-600": type === "warning",
  });
  const icon =
    type === "success" ? (
      <CircleCheckBig className={iconStyle} width={20} />
    ) : (
      <TriangleAlert className={iconStyle} width={20} />
    );

  const style = clsx({
    "border-blue-200 bg-blue-50 text-blue-600": type === "info",
    "border-green-200 bg-green-50 text-green-600": type === "success",
    "border-red-200 bg-red-50 text-red-600": type === "error",
    "border-yellow-200 bg-yellow-50 text-yellow-600": type === "warning",
  });

  return (
    <div
      className={clsx("mb-4 rounded-lg border p-3 font-medium text-sm", style)}
    >
      <div className="flex items-center gap-2">
        {icon}
        <p className="whitespace-pre-line"> {message}</p>
      </div>
    </div>
  );
}

export default MessageContainer;
