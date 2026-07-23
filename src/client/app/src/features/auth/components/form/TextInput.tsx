import clsx from "clsx";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

interface TextInputProps {
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

export const TextInput = ({
  id,
  label,
  description,
  error,
  placeholder,
  className = "",
  disabled = false,
  ref,
  name,
  onChange,
  value,
}: TextInputProps) => (
  <Field className={clsx("max-w-sm", className)}>
    <FieldLabel htmlFor={id}>{label}</FieldLabel>
    <Input
      className={clsx(
        "text-gray-950 focus-visible:border-indigo-600 focus-visible:ring-0",
        {
          "cursor-not-allowed disabled:pointer-events-auto": disabled,
        }
      )}
      disabled={disabled}
      id={id}
      name={name}
      onChange={onChange}
      placeholder={placeholder}
      ref={ref}
      type="text"
      value={value}
    />

    {!!description && <FieldDescription>{description}</FieldDescription>}

    {!!error && <FieldError>{error}</FieldError>}
  </Field>
);
