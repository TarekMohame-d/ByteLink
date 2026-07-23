-- ==============================================================================
-- TABLES
-- ==============================================================================
CREATE TABLE identity.forget_password_tokens (
  id uuid PRIMARY KEY,
  email varchar(255) NOT NULL,
  token_hash text NOT NULL,
  is_used boolean NOT NULL DEFAULT FALSE,
  expires_at_utc timestamptz NOT NULL,
  created_at_utc timestamptz NOT NULL DEFAULT NOW(),
  used_at_utc timestamptz
);

-- ==============================================================================
-- INDEXES
-- ==============================================================================
CREATE INDEX idx_forget_password_tokens_email ON identity.forget_password_tokens (email, token_hash);
