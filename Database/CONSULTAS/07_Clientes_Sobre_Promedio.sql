USE sigemic;

SELECT
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    c.profesion AS 'Profesión',
    c.lugar_trabajo AS 'Lugar de trabajo',
    c.ingreso_mensual AS 'Ingreso mensual'
FROM clientes c
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
WHERE c.ingreso_mensual >
(
    SELECT AVG(ingreso_mensual)
    FROM clientes
    WHERE ingreso_mensual IS NOT NULL
)
ORDER BY c.ingreso_mensual DESC;