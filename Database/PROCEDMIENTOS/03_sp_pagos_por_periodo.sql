USE sigemic;

DELIMITER $$

CREATE PROCEDURE sp_pagos_por_periodo(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN

    -- =====================================================
    -- VALIDAR FECHAS
    -- =====================================================

    IF p_fecha_inicio IS NULL OR p_fecha_fin IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Las fechas de inicio y fin son obligatorias';

    END IF;


    IF p_fecha_inicio > p_fecha_fin THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'La fecha inicial no puede ser mayor que la fecha final';

    END IF;


    -- =====================================================
    -- RESULTADO 1
    -- RESUMEN GENERAL DEL PERÍODO
    -- =====================================================

    SELECT
        p_fecha_inicio AS 'Fecha inicial',

        p_fecha_fin AS 'Fecha final',

        COUNT(p.id_pago) AS 'Cantidad de pagos',

        COALESCE(
            SUM(p.monto_total),
            0
        ) AS 'Total recibido',

        COALESCE(
            SUM(p.capital_aplicado),
            0
        ) AS 'Capital aplicado',

        COALESCE(
            SUM(p.interes_aplicado),
            0
        ) AS 'Interés aplicado',

        COALESCE(
            SUM(p.mora_aplicada),
            0
        ) AS 'Mora aplicada',

        COALESCE(
            AVG(p.monto_total),
            0
        ) AS 'Promedio por pago'

    FROM pagos p

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin;


    -- =====================================================
    -- RESULTADO 2
    -- RESUMEN POR FORMA DE PAGO
    -- =====================================================

    SELECT
        fp.nombre AS 'Forma de pago',

        COUNT(p.id_pago) AS 'Cantidad',

        SUM(p.monto_total) AS 'Total recibido',

        SUM(p.capital_aplicado) AS 'Capital',

        SUM(p.interes_aplicado) AS 'Interés',

        AVG(p.monto_total) AS 'Promedio'

    FROM pagos p

    INNER JOIN formas_pago fp
        ON fp.id_forma_pago = p.id_forma_pago

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin

    GROUP BY
        fp.id_forma_pago,
        fp.nombre

    ORDER BY
        SUM(p.monto_total) DESC;


    -- =====================================================
    -- RESULTADO 3
    -- DETALLE DE PAGOS DEL PERÍODO
    -- =====================================================

    SELECT
        p.numero_recibo AS 'Recibo',

        DATE(p.fecha_pago) AS 'Fecha',

        cr.codigo_credito AS 'Crédito',

        c.codigo_cliente AS 'Código cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        fp.nombre AS 'Forma de pago',

        p.tipo_operacion AS 'Operación',

        p.monto_total AS 'Monto',

        p.capital_aplicado AS 'Capital',

        p.interes_aplicado AS 'Interés',

        p.mora_aplicada AS 'Mora'

    FROM pagos p

    INNER JOIN creditos cr
        ON cr.id_credito = p.id_credito

    INNER JOIN clientes c
        ON c.id_cliente = cr.id_cliente

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    INNER JOIN formas_pago fp
        ON fp.id_forma_pago = p.id_forma_pago

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin

    ORDER BY p.fecha_pago DESC;

END$$

DELIMITER ;