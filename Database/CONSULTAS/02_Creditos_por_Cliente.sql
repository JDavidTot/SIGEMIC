USE sigemic;

SELECT
    cr.codigo_credito AS 'Crédito',
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    pc.nombre AS 'Plan',
    cr.monto_original AS 'Monto original',
    cr.tipo_tasa_aplicada AS 'Tipo de tasa',
    cr.porcentaje_interes_aplicado AS 'Interés %',
    cr.plazo_meses AS 'Plazo meses',
    cr.cuota_original AS 'Cuota',
    cr.saldo_capital AS 'Saldo capital',
    cr.estado AS 'Estado'
FROM creditos cr
INNER JOIN clientes c
    ON c.id_cliente = cr.id_cliente
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
INNER JOIN planes_credito pc
    ON pc.id_plan = cr.id_plan
ORDER BY cr.codigo_credito;