USE sigemic;

SELECT
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    COUNT(cr.id_credito) AS 'Cantidad de créditos',
    SUM(cr.monto_original) AS 'Total otorgado',
    SUM(cr.saldo_capital) AS 'Saldo pendiente'
FROM clientes c
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
INNER JOIN creditos cr
    ON cr.id_cliente = c.id_cliente
GROUP BY
    c.id_cliente,
    c.codigo_cliente,
    u.nombres,
    u.apellidos
HAVING SUM(cr.saldo_capital) > 0
ORDER BY
    SUM(cr.saldo_capital) DESC;