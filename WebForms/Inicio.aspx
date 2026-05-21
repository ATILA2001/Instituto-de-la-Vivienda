<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Inicio.aspx.cs" Inherits="WebForms.Inicio" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .plafi-home {
            max-width: 1120px;
            padding: 1.75rem 1rem;
        }

        .plafi-home__header {
            margin-bottom: 1.5rem;
        }

        .plafi-home__eyebrow {
            color: #0d6efd;
            font-size: .85rem;
            font-weight: 700;
            text-transform: uppercase;
        }

        .plafi-home__header h1 {
            margin: .35rem 0 .5rem;
        }

        .plafi-home__grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
            gap: 1rem;
        }

        .plafi-home__tile {
            display: grid;
            gap: .45rem;
            min-height: 132px;
            padding: 1rem;
            color: #212529;
            text-decoration: none;
            border: 1px solid rgba(0, 0, 0, .12);
            border-radius: 6px;
            background: #fff;
            box-shadow: 0 0.25rem 0.75rem rgba(0, 0, 0, .05);
        }

        .plafi-home__tile:hover {
            border-color: #0d6efd;
            color: #212529;
        }

        .plafi-home__tile strong {
            color: #111827;
            font-size: 1.05rem;
            font-weight: 700;
        }

        .plafi-home__tile small,
        .plafi-home__empty {
            color: #6c757d;
        }

        .plafi-home__empty {
            padding: 1rem;
            border: 1px solid rgba(0, 0, 0, .12);
            border-radius: 6px;
            background: #fff;
        }

        body.dark-mode .plafi-home__tile,
        body.dark-mode .plafi-home__empty {
            color: #f8f9fa;
            border-color: rgba(255, 255, 255, .2);
            background: #212529;
        }

        body.dark-mode .plafi-home__tile:hover {
            color: #f8f9fa;
        }

        body.dark-mode .plafi-home__tile strong {
            color: #fff;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section class="container plafi-home">
        <div class="plafi-home__header">
            <span class="plafi-home__eyebrow">PLAFI</span>
            <h1>Planificacion y Formulacion</h1>
            <p class="text-muted mb-0">Selecciona una seccion disponible para comenzar.</p>
        </div>

        <asp:Panel ID="pnlEmpty" CssClass="plafi-home__empty" Visible="false" runat="server">
            No tenes paginas disponibles para esta aplicacion.
        </asp:Panel>

        <div class="plafi-home__grid">
            <asp:Repeater ID="rptPages" runat="server">
                <ItemTemplate>
                    <a class="plafi-home__tile" href="<%# Eval("Url") %>">
                        <strong><%# Eval("Title") %></strong>
                        <small><%# Eval("Description") %></small>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </section>
</asp:Content>
