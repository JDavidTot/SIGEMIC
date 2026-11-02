USE sigemic;

SELECT
    cr.codigo_credito AS 'Crédito',
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    cu.numero_cuota AS 'Cuota',
    cu.fecha_vencimiento AS 'Fecha de vencimiento',
    cu.saldo_cuota AS 'Saldo pendiente',
    cu.estado AS 'Estado',
    DATEDIFF(CURRENT_DATE, cu.fecha_vencimiento) AS 'Días vencidos'
FROM cuotas cu
INNER JOIN creditos cr
    ON cr.id_credito = cu.id_credito
INNER JOIN clientes c
    ON c.id_cliente = cr.id_cliente
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
WHERE
    cu.fecha_vencimiento < CURRENT_DATE
    AND cu.estado <> 'PAGADA'
ORDER BY
    cu.fecha_vencimiento;