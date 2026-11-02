USE sigemic;

SELECT
    c.codigo_cliente AS 'Código',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    u.username AS 'Usuario',
    c.profesion AS 'Profesión',
    c.lugar_trabajo AS 'Lugar de trabajo',
    c.ingreso_mensual AS 'Ingreso mensual',
    c.estado AS 'Estado'
FROM clientes c
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
ORDER BY c.codigo_cliente;