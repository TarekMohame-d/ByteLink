import { useNavigate, useSearch } from "@tanstack/react-router";
import { useCallback, useState } from "react";
import BackgroundGradient from "@/components/BackgroundGradient";
import { ExpiredState } from "./ExpiredState";
import { IdleState } from "./IdleState";
import { SuccessState } from "./SuccessState";
import { VerifyingState } from "./VerifyingState";

function VerifyEmail() {
  const search = useSearch({ from: "/auth/verify-email" });
  const navigate = useNavigate({ from: "/auth/verify-email" });

  const [status, setStatus] = useState<
    "verifying" | "success" | "expired" | "idle"
  >(search.token ? "verifying" : "idle");

  const handleExpiredSuccess = useCallback(
    (email: string) => {
      setStatus("idle");
      navigate({
        search: { email, token: undefined },
        to: "/auth/verify-email",
      });
    },
    [navigate]
  );

  return (
    <div className="relative flex min-h-screen flex-col items-center justify-center p-4">
      <BackgroundGradient />

      {status === "verifying" && (
        <VerifyingState setStatus={setStatus} token={search.token} />
      )}
      {status === "success" && <SuccessState email={search.email} />}
      {status === "expired" && (
        <ExpiredState
          initialEmail={search.email}
          onSuccess={handleExpiredSuccess}
        />
      )}
      {status === "idle" && <IdleState email={search.email} />}
    </div>
  );
}

export default VerifyEmail;
