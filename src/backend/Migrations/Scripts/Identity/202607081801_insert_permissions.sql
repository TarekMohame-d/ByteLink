INSERT INTO
    identity.roles (id, name)
VALUES
    (1, 'admin'),
    (2, 'user');

INSERT INTO
    identity.permissions (id, name)
VALUES
    (1, 'create'),
    (2, 'read'),
    (3, 'update'),
    (4, 'delete');

INSERT INTO
    identity.role_permissions (role_id, permission_id)
VALUES
    (1, 1),
    (1, 2),
    (1, 3),
    (1, 4),
    (2, 2);
