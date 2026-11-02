USE sigemic;

SELECT
    estado AS 'Estado del crédito',
    COUNT(*) AS 'Cantidad de créditos',
    SUM(monto_original) AS 'Total otorgado',
    SUM(saldo_capital) AS 'Saldo pendiente',
    AVG(monto_original) AS 'Monto promedio'
FROM creditos
GROUP BY estado
ORDER BY COUNT(*) DESC;