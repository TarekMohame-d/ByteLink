import clsx from "clsx";
import { Mail } from "lucide-react";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";

interface EmailInputProps {
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

export const EmailInput = ({
  id,
  label,
  description,
  error,
  placeholder = "name@example.com",
  className = "",
  disabled = false,
  ref,
  name,
  onChange,
  value,
}: EmailInputProps) => (
  <Field className={clsx("max-w-sm", className)}>
    <FieldLabel htmlFor={id}>{label}</FieldLabel>
    <InputGroup
      className={clsx(
        "text-gray-950 focus-within:border-indigo-600 focus-within:ring-0 focus-within:ring-offset-0",
        {
          "cursor-not-allowed disabled:pointer-events-auto": disabled,
        }
      )}
    >
      <InputGroupAddon align="inline-start">
        <Mail className="h-4 w-4 text-indigo-600" />
      </InputGroupAddon>
      <InputGroupInput
        disabled={disabled}
        id={id}
        name={name}
        onChange={onChange}
        placeholder={placeholder}
        ref={ref}
        type="email"
        value={value}
      />
    </InputGroup>

    {!!description && <FieldDescription>{description}</FieldDescription>}

    {!!error && <FieldError>{error}</FieldError>}
  </Field>
);
