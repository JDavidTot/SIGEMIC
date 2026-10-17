USE sigemic;

-- ============================================================
-- SIGEMIC v1.6.0
-- CATÁLOGO DE PERMISOS PARA CAJEROS
-- ============================================================

CREATE TABLE IF NOT EXISTS permisos
(
    id_permiso INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(80) NOT NULL,
    nombre VARCHAR(120) NOT NULL,
    descripcion VARCHAR(255) NULL,
    modulo VARCHAR(50) NOT NULL,
    estado ENUM('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_permisos_codigo UNIQUE (codigo)
);

CREATE TABLE IF NOT EXISTS usuario_permisos
(
    id_usuario INT UNSIGNED NOT NULL,
    id_permiso INT UNSIGNED NOT NULL,
    fecha_asignacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id_usuario, id_permiso),

    CONSTRAINT fk_usuario_permisos_usuario
        FOREIGN KEY (id_usuario)
        REFERENCES usuarios(id_usuario)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT fk_usuario_permisos_permiso
        FOREIGN KEY (id_permiso)
        REFERENCES permisos(id_permiso)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
);

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CLIENTES_VER',
 'Ver clientes',
 'Permite consultar el listado y detalle de clientes.',
 'CLIENTES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CLIENTES_CREAR',
 'Crear clientes',
 'Permite registrar nuevos clientes en SIGEMIC.',
 'CLIENTES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CLIENTES_EDITAR',
 'Editar clientes',
 'Permite modificar la información de clientes registrados.',
 'CLIENTES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CLIENTES_CAMBIAR_ESTADO',
 'Cambiar estado de clientes',
 'Permite activar o inactivar clientes.',
 'CLIENTES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CREDITOS_VER',
 'Ver créditos',
 'Permite consultar el listado de créditos.',
 'CREDITOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CREDITOS_CREAR',
 'Crear créditos',
 'Permite otorgar nuevos créditos a clientes.',
 'CREDITOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('CREDITOS_DETALLE',
 'Ver detalle de créditos',
 'Permite consultar el detalle y tabla de amortización de un crédito.',
 'CREDITOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('PAGOS_REGISTRAR',
 'Registrar pagos',
 'Permite registrar pagos completos o parciales de cuotas.',
 'PAGOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('PAGOS_ABONO_CAPITAL',
 'Registrar abonos a capital',
 'Permite realizar abonos extraordinarios directamente al capital.',
 'PAGOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('PAGOS_HISTORIAL',
 'Ver historial de pagos',
 'Permite consultar pagos, recibos y abonos registrados.',
 'PAGOS',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('SOLICITUDES_VER',
 'Ver solicitudes de pago',
 'Permite consultar solicitudes y comprobantes enviados por clientes.',
 'SOLICITUDES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('SOLICITUDES_APROBAR',
 'Aprobar solicitudes',
 'Permite aprobar solicitudes y aplicar el pago correspondiente.',
 'SOLICITUDES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

INSERT INTO permisos
(codigo, nombre, descripcion, modulo, estado)
VALUES
('SOLICITUDES_RECHAZAR',
 'Rechazar solicitudes',
 'Permite rechazar solicitudes de pago indicando un motivo.',
 'SOLICITUDES',
 'ACTIVO')
ON DUPLICATE KEY UPDATE
nombre = VALUES(nombre),
descripcion = VALUES(descripcion),
modulo = VALUES(modulo),
estado = 'ACTIVO';

-- ============================================================
-- VERIFICACIÓN
-- ============================================================

SELECT
    id_permiso,
    modulo,
    codigo,
    nombre,
    estado
FROM permisos
ORDER BY modulo, nombre;

SELECT COUNT(*) AS total_permisos_activos
FROM permisos
WHERE estado = 'ACTIVO';