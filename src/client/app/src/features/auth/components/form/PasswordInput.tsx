import clsx from "clsx";
import { Eye, EyeOffIcon } from "lucide-react";
import { useCallback, useState } from "react";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";

interface PasswordInputProps {
  className?: string;
  description?: string;
  disabled?: boolean;
  error?: string;
  id?: string;
  label?: string;
  name?: string;
  onChange?: React.ChangeEventHandler<HTMLInputElement>;
  placeholder?: string;
  ref?: React.RefObject<HTMLInputElement | null>;
  value?: string;
}

export const PasswordInput = ({
  id,
  label,
  description,
  error,
  disabled,
  className,
  placeholder = "Password...",
  ref,
  name,
  onChange,
  value,
}: PasswordInputProps) => {
  const [showPassword, setShowPassword] = useState(false);

  const toggleShowPassword = useCallback(() => {
    setShowPassword((prev) => !prev);
  }, []);

  return (
    <Field className={clsx("max-w-sm", className)}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <InputGroup className="text-gray-950">
        <InputGroupInput
          disabled={disabled}
          id={id}
          name={name}
          onChange={onChange}
          placeholder={placeholder}
          ref={ref}
          type={showPassword ? "text" : "password"}
          value={value}
        />
        <InputGroupAddon align="inline-end">
          <InputGroupButton onClick={toggleShowPassword} size="sm">
            {showPassword ? (
              <EyeOffIcon
                className="h-4 w-4 cursor-pointer text-indigo-600"
                onClick={toggleShowPassword}
              />
            ) : (
              <Eye
                className="h-4 w-4 cursor-pointer text-indigo-600"
                onClick={toggleShowPassword}
              />
            )}
          </InputGroupButton>
        </InputGroupAddon>
      </InputGroup>
      {!!description && <FieldDescription>{description}</FieldDescription>}

      {!!error && <FieldError>{error}</FieldError>}
    </Field>
  );
};
