USE sigemic;

DELIMITER $$

CREATE TRIGGER trg_solicitudes_pago_validar_update
BEFORE UPDATE ON solicitudes_pago
FOR EACH ROW
BEGIN

    -- =====================================================
    -- 1. NO PERMITIR CAMBIAR EL ESTADO DE UNA SOLICITUD QUE YA FUE REVISADA
    -- =====================================================

    IF OLD.estado IN ('APROBADA', 'RECHAZADA')
       AND NEW.estado <> OLD.estado THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La solicitud ya fue revisada y su estado no puede modificarse';

    END IF;


    -- =====================================================
    -- 2. VALIDACIONES PARA APROBAR
    -- =====================================================

    IF NEW.estado = 'APROBADA' THEN

        IF NEW.id_pago IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe tener un pago asociado';

        END IF;


        IF NEW.id_usuario_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe indicar el usuario revisor';

        END IF;


        IF NEW.fecha_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe registrar fecha de revisión';

        END IF;

    END IF;


    -- =====================================================
    -- 3. VALIDACIONES PARA RECHAZAR
    -- =====================================================

    IF NEW.estado = 'RECHAZADA' THEN

        IF NEW.id_pago IS NOT NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada no puede tener un pago asociado';

        END IF;


        IF NEW.id_usuario_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe indicar el usuario revisor';

        END IF;


        IF NEW.fecha_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe registrar fecha de revisión';

        END IF;


        IF NEW.motivo_rechazo IS NULL
           OR TRIM(NEW.motivo_rechazo) = '' THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe indicar el motivo del rechazo';

        END IF;

    END IF;

END$$

DELIMITER ;