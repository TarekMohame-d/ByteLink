import { useSearch } from "@tanstack/react-router";
import { useState } from "react";
import BackgroundGradient from "@/components/BackgroundGradient";
import ForgotPassword from "./ForgotPassword";
import SetNewPassword from "./SetNewPassword";
import SuccessResetPassword from "./SuccessResetPassword";

function ResetPassword() {
  const search = useSearch({ from: "/auth/reset-password" });
  const [status, setStatus] = useState<"email" | "password" | "success">(
    search.token ? "password" : "email"
  );
  return (
    <div className="relative flex min-h-screen flex-col items-center justify-center p-4">
      <BackgroundGradient />

      {status === "email" && <ForgotPassword />}
      {status === "password" && <SetNewPassword setStatus={setStatus} />}
      {status === "success" && <SuccessResetPassword />}
    </div>
  );
}

export default ResetPassword;
