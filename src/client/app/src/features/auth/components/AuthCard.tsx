import clsx from "clsx";

interface ComponentProps {
  children: React.ReactNode;
  className?: string;
}

export const AuthCard = ({ children, className = "" }: ComponentProps) => (
  <div
    className={clsx(
      "flex w-full flex-col gap-6 rounded-2xl border border-gray-200 bg-white p-6 shadow-2xl md:w-2/5 md:p-8",
      className
    )}
  >
    {children}
  </div>
);

AuthCard.Header = function VerifyCardHeader({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div className={clsx("flex flex-row justify-start gap-3", className)}>
      {children}
    </div>
  );
};

AuthCard.Body = function VerifyCardBody({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div className={clsx("flex w-full flex-col items-center gap-4", className)}>
      {children}
    </div>
  );
};

AuthCard.Footer = function VerifyCardFooter({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div className={clsx("flex flex-row justify-center", className)}>
      {children}
    </div>
  );
};
