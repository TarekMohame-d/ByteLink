import { LinkIcon, Loader2 } from "lucide-react";
import { useEffect } from "react";
import { useVerifyEmail } from "../../hooks/use-verify";
import { AuthCard } from "../AuthCard";

interface VerifyingStateProps {
  setStatus: (status: "verifying" | "success" | "expired" | "idle") => void;
  token?: string;
}

export function VerifyingState({ setStatus, token }: VerifyingStateProps) {
  const { mutate } = useVerifyEmail();

  useEffect(() => {
    if (!token) {
      return;
    }

    const timer = setTimeout(() => {
      mutate(
        { token },
        {
          onError: () => {
            setStatus("expired");
          },
          onSuccess: () => {
            setStatus("success");
          },
        }
      );
    }, 1000);

    return () => clearTimeout(timer);
  }, [token, mutate, setStatus]);

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
          <Loader2 className="animate-spin" size={40} />
        </div>
        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 md:text-3xl">
            Verifying your token
          </h1>
          <p className="text-gray-500">
            Please wait a moment while we process your account activation...
          </p>
        </div>
      </AuthCard.Body>
    </AuthCard>
  );
}
