USE sigemic;
SELECT
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    COUNT(p.id_pago) AS 'Cantidad de pagos',
    SUM(p.monto_total) AS 'Total pagado',
    SUM(p.capital_aplicado) AS 'Capital pagado',
    SUM(p.interes_aplicado) AS 'Interés pagado'
FROM clientes c
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
INNER JOIN creditos cr
    ON cr.id_cliente = c.id_cliente
INNER JOIN pagos p
    ON p.id_credito = cr.id_credito
GROUP BY
    c.id_cliente,
    c.codigo_cliente,
    u.nombres,
    u.apellidos
ORDER BY
    SUM(p.monto_total) DESC;