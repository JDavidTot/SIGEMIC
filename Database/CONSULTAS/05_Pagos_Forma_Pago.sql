USE sigemic;

SELECT
    fp.nombre AS 'Forma de pago',
    COUNT(p.id_pago) AS 'Cantidad de operaciones',
    SUM(p.monto_total) AS 'Total recibido',
    AVG(p.monto_total) AS 'Promedio por operación'
FROM pagos p
INNER JOIN formas_pago fp
    ON fp.id_forma_pago = p.id_forma_pago
GROUP BY
    fp.id_forma_pago,
    fp.nombre
ORDER BY
    SUM(p.monto_total) DESC;