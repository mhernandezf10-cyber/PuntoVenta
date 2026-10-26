window.aresFacturas = {
    imprimirCompra: function (factura) {
        this.generarFactura(factura, "COMPRA");
    },

    imprimirVenta: function (factura) {
        this.generarFactura(factura, "VENTA");
    },

    generarFactura: function (factura, tipo) {
        const escapar = valor =>
            String(valor ?? "").replace(/[&<>"']/g, c => ({
                "&": "&amp;",
                "<": "&lt;",
                ">": "&gt;",
                '"': "&quot;",
                "'": "&#39;"
            })[c]);

        const moneda = valor =>
            "Q " + Number(valor ?? 0).toLocaleString("es-GT", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });

        const ventana = window.open("about:blank", "_blank");

        if (!ventana) {
            alert(
                "El navegador bloqueó la factura. " +
                "Permite las ventanas emergentes para localhost e inténtalo de nuevo."
            );
            return;
        }

        const esCompra = tipo === "COMPRA";
        const titulo = esCompra ? "FACTURA DE COMPRA" : "FACTURA DE VENTA";
        const tercero = esCompra ? "Proveedor" : "Cliente";
        const nombreTercero = esCompra ? factura.proveedor : factura.cliente;

        const filas = (factura.items ?? []).map(item => `
            <tr>
                <td>${escapar(item.producto)}</td>
                <td>${escapar(item.cantidad)}</td>
                <td>${moneda(item.precio)}</td>
                <td>${moneda(item.subtotal)}</td>
            </tr>
        `).join("");

        ventana.document.open();
        ventana.document.write(`
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>${titulo} ${escapar(factura.numero)}</title>
                <style>
                    * { box-sizing: border-box; }

                    body {
                        font-family: Arial, sans-serif;
                        margin: 35px;
                        color: #222;
                    }

                    header {
                        border-bottom: 3px solid #c91420;
                        padding-bottom: 15px;
                    }

                    h1 { color: #c91420; }

                    .datos {
                        display: flex;
                        justify-content: space-between;
                        gap: 20px;
                        margin: 25px 0;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                    }

                    th, td {
                        border-bottom: 1px solid #ddd;
                        padding: 12px 8px;
                        text-align: left;
                    }

                    th { background: #f1f1f1; }

                    .total {
                        text-align: right;
                        font-size: 20px;
                        font-weight: bold;
                        margin-top: 25px;
                    }

                    .nota {
                        text-align: center;
                        color: #666;
                        font-size: 12px;
                        margin-top: 35px;
                    }

                    .botones {
                        text-align: center;
                        margin-top: 30px;
                    }

                    button {
                        background: #c91420;
                        color: white;
                        border: 0;
                        border-radius: 5px;
                        padding: 12px 22px;
                        font-size: 15px;
                        cursor: pointer;
                    }

                    @page {
                        size: A4;
                        margin: 15mm;
                    }

                    @media print {
                        .botones { display: none !important; }
                        body { margin: 0; }
                    }
                </style>
            </head>
            <body>
                <header>
                    <h1>ARES TECH</h1>
                    <p>El fin de las amenazas. El inicio del control.</p>
                    <small>Comprobante interno de ${esCompra ? "compra" : "venta"}</small>
                </header>

                <h2>${titulo}</h2>

                <div class="datos">
                    <div>
                        <strong>Número:</strong> ${escapar(factura.numero)}<br>
                        <strong>Fecha:</strong> ${escapar(factura.fecha)}
                    </div>
                    <div>
                        <strong>${tercero}:</strong><br>
                        ${escapar(nombreTercero)}
                    </div>
                </div>

                <table>
                    <thead>
                        <tr>
                            <th>Producto</th>
                            <th>Cantidad</th>
                            <th>Precio unitario</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>${filas}</tbody>
                </table>

                <div class="total">
                    TOTAL: ${moneda(factura.total)}
                </div>

                <p class="nota">
                    Comprobante interno de ARES TECH.
                    No sustituye una factura tributaria autorizada.
                </p>

                <div class="botones">
                    <button id="btnImprimir" type="button">
                        Imprimir / Guardar como PDF
                    </button>
                </div>
            </body>
            </html>
        `);

        ventana.document.close();

        // Se asigna directamente, sin esperar al evento load.
        ventana.document.getElementById("btnImprimir").onclick = function () {
            ventana.focus();
            ventana.print();
        };

        ventana.focus();
    }
};