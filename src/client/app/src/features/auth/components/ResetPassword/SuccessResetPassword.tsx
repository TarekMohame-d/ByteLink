import { useNavigate } from "@tanstack/react-router";
import { CircleCheckBig, LinkIcon } from "lucide-react";
import { useCallback } from "react";
import { AuthCard } from "../AuthCard";

function SuccessResetPassword() {
  const navigate = useNavigate({ from: "/auth/reset-password" });

  const handleClick = useCallback(() => {
    navigate({ to: "/auth/sign-in" });
  }, [navigate]);

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
          <CircleCheckBig size={40} />
        </div>

        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 md:text-3xl">
            Password Reset!
          </h1>
          <p className="mx-auto max-w-sm text-gray-500 text-sm">
            Your password has been successfully reset, click below to continue
            your access.
          </p>
        </div>

        <button
          className="w-full max-w-sm rounded-md bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-500"
          onClick={handleClick}
          type="button"
        >
          Continue
        </button>
      </AuthCard.Body>
    </AuthCard>
  );
}

export default SuccessResetPassword;
