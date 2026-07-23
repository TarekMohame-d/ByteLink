import clsx from "clsx";
import { Button } from "@/components/ui/button";

interface SubmitButtonProps {
  className?: string;
  disabled?: boolean;
  label: string;
  onClick?: () => void;
}

export const SubmitButton = ({
  label,
  disabled = false,
  className = "",
  onClick,
}: SubmitButtonProps) => (
  <Button
    className={clsx(
      "w-full rounded-md bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-500 disabled:bg-indigo-400",
      {
        className,
        "cursor-not-allowed disabled:pointer-events-auto": disabled,
        "cursor-pointer": !disabled,
      }
    )}
    disabled={disabled}
    onClick={onClick}
    type="submit"
  >
    {label}
  </Button>
);
