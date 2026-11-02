USE sigemic;

DELIMITER $$

CREATE PROCEDURE sp_resumen_credito(
    IN p_id_credito BIGINT UNSIGNED
)
BEGIN

    -- =====================================================
    -- VALIDAR QUE EL CRÉDITO EXISTA
    -- =====================================================

    IF NOT EXISTS (
        SELECT 1
        FROM creditos
        WHERE id_credito = p_id_credito
    ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El crédito indicado no existe';

    END IF;


    -- =====================================================
    -- RESULTADO 1
    -- RESUMEN FINANCIERO DEL CRÉDITO
    -- =====================================================

    SELECT
        cr.id_credito AS 'ID Crédito',

        cr.codigo_credito AS 'Código crédito',

        c.codigo_cliente AS 'Código cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        pc.nombre AS 'Plan',

        cr.monto_original AS 'Monto original',

        cr.tipo_tasa_aplicada AS 'Tipo de tasa',

        cr.porcentaje_interes_aplicado AS 'Interés %',

        cr.plazo_meses AS 'Plazo meses',

        cr.cuota_original AS 'Cuota original',

        cr.saldo_capital AS 'Saldo capital',

        cr.estado AS 'Estado',

        cr.fecha_otorgamiento AS 'Fecha otorgamiento',

        cr.fecha_inicio AS 'Fecha inicio',

        cr.fecha_finalizacion_estimada AS 'Finalización estimada',

        COALESCE(
            (
                SELECT SUM(p.monto_total)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Total pagado',

        COALESCE(
            (
                SELECT SUM(p.capital_aplicado)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Capital pagado',

        COALESCE(
            (
                SELECT SUM(p.interes_aplicado)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Interés pagado',

        COALESCE(
            (
                SELECT SUM(p.mora_aplicada)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Mora pagada',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            WHERE
                cu.id_credito = cr.id_credito
                AND cu.estado = 'PAGADA'
        ) AS 'Cuotas pagadas',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            WHERE
                cu.id_credito = cr.id_credito
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Cuotas pendientes'

    FROM creditos cr

    INNER JOIN clientes c
        ON c.id_cliente = cr.id_cliente

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    INNER JOIN planes_credito pc
        ON pc.id_plan = cr.id_plan

    WHERE cr.id_credito = p_id_credito;


    -- =====================================================
    -- RESULTADO 2
    -- DETALLE DE CUOTAS / TABLA DE AMORTIZACIÓN
    -- =====================================================

    SELECT
        cu.numero_cuota AS 'Cuota',

        cu.fecha_vencimiento AS 'Vencimiento',

        cu.capital_programado AS 'Capital programado',

        cu.interes_programado AS 'Interés programado',

        cu.cuota_programada AS 'Cuota programada',

        cu.capital_pagado AS 'Capital pagado',

        cu.interes_pagado AS 'Interés pagado',

        cu.mora_pagada AS 'Mora pagada',

        cu.saldo_cuota AS 'Saldo cuota',

        cu.estado AS 'Estado'

    FROM cuotas cu

    WHERE cu.id_credito = p_id_credito

    ORDER BY cu.numero_cuota;

END$$

DELIMITER ;