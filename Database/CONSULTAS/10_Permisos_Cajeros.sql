USE sigemic;

SELECT
    u.username AS 'Usuario',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Nombre',
    p.modulo AS 'Módulo',
    p.codigo AS 'Código del permiso',
    p.nombre AS 'Permiso'
FROM usuario_permisos up
INNER JOIN usuarios u
    ON u.id_usuario = up.id_usuario
INNER JOIN permisos p
    ON p.id_permiso = up.id_permiso
WHERE u.rol = 'CAJERO'
ORDER BY
    u.username,
    p.modulo,
    p.codigo;