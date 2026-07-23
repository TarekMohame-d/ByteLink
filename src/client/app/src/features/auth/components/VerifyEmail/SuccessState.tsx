import { useNavigate } from "@tanstack/react-router";
import { CheckCircle2, LinkIcon } from "lucide-react";
import { useEffect } from "react";
import { AuthCard } from "../AuthCard";

interface SuccessStateProps {
  email: string;
}

export function SuccessState({ email }: SuccessStateProps) {
  const navigate = useNavigate({ from: "/auth/verify-email" });

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate({
        search: { email },
        to: "/auth/sign-in",
      });
    }, 2000);

    return () => clearTimeout(timer);
  }, [navigate, email]);

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
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-green-50 text-green-600">
          <CheckCircle2 size={40} />
        </div>
        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 md:text-3xl">
            Account Verified!
          </h1>
          <p className="font-medium text-green-600">
            Success! Redirecting you to home page...
          </p>
        </div>
      </AuthCard.Body>
    </AuthCard>
  );
}
