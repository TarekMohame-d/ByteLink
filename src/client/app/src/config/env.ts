import z from "zod";

const createEnv = () => {
  const EnvSchema = z.object({
    // BASE_URL: z.url("Must be a valid URL structure"),
  });

  const envVars = {
    // BASE_URL: import.meta.env.VITE_APP_BASE_URL,
  };

  const parsedEnv = EnvSchema.safeParse(envVars);

  if (!parsedEnv.success) {
    throw new Error(
      "Invalid env provided. The following variables are missing or invalid:\n" +
        Object.entries(parsedEnv.error.flatten().fieldErrors)
          .map(([k, v]) => `- ${k}: ${v?.join(", ")}`)
          .join("\n")
    );
  }

  return parsedEnv.data;
};

export const env = createEnv();
