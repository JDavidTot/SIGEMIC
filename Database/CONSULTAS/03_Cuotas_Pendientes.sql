USE sigemic;
SELECT
    cr.codigo_credito AS 'Crédito',
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    cu.numero_cuota AS 'Cuota',
    cu.fecha_vencimiento AS 'Vencimiento',
    cu.capital_programado AS 'Capital',
    cu.interes_programado AS 'Interés',
    cu.cuota_programada AS 'Cuota programada',
    cu.saldo_cuota AS 'Saldo pendiente',
    cu.estado AS 'Estado'
FROM cuotas cu
INNER JOIN creditos cr
    ON cr.id_credito = cu.id_credito
INNER JOIN clientes c
    ON c.id_cliente = cr.id_cliente
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
WHERE cu.estado IN ('PENDIENTE', 'PARCIAL', 'ATRASADA')
ORDER BY
    cu.fecha_vencimiento,
    cr.codigo_credito,
    cu.numero_cuota;