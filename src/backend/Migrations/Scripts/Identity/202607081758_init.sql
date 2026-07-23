-- ==============================================================================
-- SCHEMA & UTILITIES
-- ==============================================================================
CREATE SCHEMA IF NOT EXISTS identity;

CREATE OR REPLACE FUNCTION identity.set_updated_at_utc () RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at_utc := NOW();
    RETURN NEW;
END;
$$;

-- ==============================================================================
-- TABLES
-- ==============================================================================
CREATE TABLE identity.roles (
  id smallint PRIMARY KEY,
  name varchar(25) NOT NULL
);

CREATE TABLE identity.permissions (
  id smallint PRIMARY KEY,
  name varchar(25) NOT NULL
);

CREATE TABLE identity.role_permissions (
  role_id smallint NOT NULL REFERENCES identity.roles (id) ON DELETE CASCADE,
  permission_id smallint NOT NULL REFERENCES identity.permissions (id) ON DELETE CASCADE,
  PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE identity.users (
  id uuid PRIMARY KEY,
  first_name varchar(25) NOT NULL,
  last_name varchar(25) NOT NULL,
  email varchar(255) NOT NULL UNIQUE,
  password_hash text NOT NULL,
  user_role smallint NOT NULL REFERENCES identity.roles (id),
  email_verified boolean NOT NULL DEFAULT FALSE,
  is_active boolean NOT NULL DEFAULT FALSE,
  is_deleted boolean NOT NULL DEFAULT FALSE,
  deleted_at_utc timestamptz,
  created_at_utc timestamptz NOT NULL DEFAULT NOW(),
  updated_at_utc timestamptz
);

CREATE TABLE identity.email_verification_tokens (
  id uuid PRIMARY KEY,
  email varchar(255) NOT NULL UNIQUE,
  token_hash text NOT NULL,
  is_used boolean NOT NULL DEFAULT FALSE,
  expires_at_utc timestamptz NOT NULL,
  created_at_utc timestamptz NOT NULL DEFAULT NOW(),
  used_at_utc timestamptz
);

CREATE TABLE identity.user_refresh_tokens (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL REFERENCES identity.users (id) ON DELETE CASCADE,
  token_hash text NOT NULL,
  is_revoked boolean NOT NULL DEFAULT FALSE,
  device_id VARCHAR(100) NOT NULL DEFAULT 'unknown',
  device_metadata VARCHAR(255),
  expires_at_utc timestamptz NOT NULL,
  created_at_utc timestamptz NOT NULL DEFAULT NOW()
);

-- ==============================================================================
-- TRIGGERS
-- ==============================================================================
CREATE TRIGGER set_users_updated_at
BEFORE UPDATE ON identity.users FOR EACH ROW
EXECUTE FUNCTION identity.set_updated_at_utc ();

-- ==============================================================================
-- INDEXES
-- ==============================================================================
CREATE INDEX idx_user_refresh_tokens_user_id ON identity.user_refresh_tokens (token_hash, user_id, device_id);

CREATE INDEX idx_email_verification_tokens_email ON identity.email_verification_tokens (email, token_hash);

CREATE INDEX idx_user_refresh_tokens_token_hash ON identity.user_refresh_tokens (token_hash, user_id);

-- ==============================================================================
-- VIEWS
-- ==============================================================================
CREATE OR REPLACE VIEW identity.vw_users AS
SELECT
  id AS user_id,
  first_name || ' ' || last_name AS display_name,
  created_at_utc
FROM
  identity.users
WHERE
  is_active = TRUE
  AND is_deleted = FALSE;
