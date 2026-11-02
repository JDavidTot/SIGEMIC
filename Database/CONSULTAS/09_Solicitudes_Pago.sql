USE sigemic;

SELECT
    sp.id_solicitud AS 'Solicitud',
    sp.fecha_solicitud AS 'Fecha',
    c.codigo_cliente AS 'Código cliente',
    CONCAT(u.nombres, ' ', u.apellidos) AS 'Cliente',
    cr.codigo_credito AS 'Crédito',
    sp.tipo_operacion AS 'Tipo de operación',
    sp.monto AS 'Monto',
    b.nombre AS 'Banco',
    sp.numero_boleta AS 'Número de boleta',
    sp.estado AS 'Estado'
FROM solicitudes_pago sp
INNER JOIN clientes c
    ON c.id_cliente = sp.id_cliente
INNER JOIN usuarios u
    ON u.id_usuario = c.id_usuario
INNER JOIN creditos cr
    ON cr.id_credito = sp.id_credito
INNER JOIN bancos b
    ON b.id_banco = sp.id_banco
ORDER BY sp.fecha_solicitud DESC;