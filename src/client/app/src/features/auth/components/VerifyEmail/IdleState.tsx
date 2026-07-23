import clsx from "clsx";
import { LinkIcon, MailOpen } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { AuthCard } from "../AuthCard";

interface IdleStateProps {
  email: string;
}

export function IdleState({ email }: IdleStateProps) {
  const [countdown, setCountdown] = useState<number>(0);

  useEffect(() => {
    if (countdown <= 0) {
      return;
    }

    const timer = setTimeout(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearTimeout(timer);
  }, [countdown]);

  const handleResend = useCallback(() => {
    setCountdown(60);
  }, []);

  return (
    <AuthCard>
      <AuthCard.Header>
        <div className="flex h-8 w-8 items-center justify-center rounded-sm border border-gray-500 bg-indigo-600">
          <LinkIcon color="#ffffff" size={16} />
        </div>
        <span className="translate-y-0.5 font-chains text-4xl text-indigo-600 leading-none">
          BYTELINK
        </span>
      </AuthCard.Header>

      <AuthCard.Body>
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
          <MailOpen size={40} />
        </div>

        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 leading-none tracking-tight md:text-3xl">
            Verify your email address
          </h1>
          <p className="text-gray-500">
            Account activation link has been sent to{" "}
            <span className="font-medium text-gray-800">
              {email || "your email"}
            </span>
            .
          </p>
        </div>
      </AuthCard.Body>
      <AuthCard.Footer>
        <p className="flex items-center justify-center gap-1 text-gray-400 text-sm">
          Didn&apos;t get the mail?
          <button
            className={clsx("font-semibold transition-colors", {
              "cursor-not-allowed text-gray-500": countdown > 0,
              "cursor-pointer text-indigo-600 hover:underline": countdown === 0,
            })}
            disabled={countdown > 0}
            onClick={handleResend}
            type="button"
          >
            {countdown > 0 ? `Resend in ${countdown}s` : "Resend"}
          </button>
        </p>
      </AuthCard.Footer>
    </AuthCard>
  );
}
