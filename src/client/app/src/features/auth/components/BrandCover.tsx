import clsx from "clsx";

interface ComponentProps {
  children: React.ReactNode;
  className?: string;
}

export const BrandCover = ({ children, className = "" }: ComponentProps) => (
  <div
    className={clsx(
      "hidden flex-col rounded-tr-[120px] rounded-br-[60px] bg-linear-to-br from-indigo-800 via-indigo-600 to-indigo-800 p-6 text-white md:flex",
      className
    )}
  >
    {children}
  </div>
);

BrandCover.Header = function BrandCoverHeader({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div className={clsx("flex flex-row justify-start gap-3", className)}>
      {children}
    </div>
  );
};

BrandCover.Body = function BrandCoverBody({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div
      className={clsx(
        "flex flex-1 flex-col justify-center gap-4 text-left",
        className
      )}
    >
      {children}
    </div>
  );
};

BrandCover.Footer = function BrandCoverFooter({
  children,
  className = "",
}: ComponentProps) {
  return (
    <div className={clsx("flex flex-row justify-start", className)}>
      {children}
    </div>
  );
};
