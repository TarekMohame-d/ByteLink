INSERT INTO
  identity.users (
    id,
    first_name,
    last_name,
    email,
    password_hash,
    user_role,
    email_verified,
    is_active,
    is_deleted
  )
VALUES
  (
    '$adminUserId$',
    '$adminFirstName$',
    '$adminLastName$',
    '$adminEmail$',
    '$adminPasswordHash$',
    1,
    TRUE,
    TRUE,
    FALSE
  );
