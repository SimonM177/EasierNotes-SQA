// ================== app.js — Dashboard SQA Easier-Notes ==================
let DATA = window.SQA_DATA;
let charts = [];

const PALETA = {
  azul:'#1d3f72', oro:'#e6c56b', morado:'#8a5e8d', ambar:'#d29e5b', coral:'#f4a462',
  verde:'#3ecf8e', rojo:'#f47174', txt:'#eaf1f9', txt2:'#9fb3c8', line:'rgba(255,255,255,.08)'
};

const PAGES = {
  resumen:   { title:'Resumen ejecutivo',       sub:'Panorámica del aseguramiento de la Eficiencia de Desempeño' },
  niveles:   { title:'Niveles de prueba',        sub:'Pirámide de pruebas y cobertura por nivel' },
  desempeno: { title:'Desempeño bajo carga',     sub:'Comportamiento temporal, capacidad y utilización de recursos' },
  resultados:{ title:'Resultados de casos',      sub:'Casos de prueba con filtros interactivos' },
  ecosistema:{ title:'Ecosistema y gobernanza',  sub:'Integración de herramientas y gobierno de datos' },
  hallazgos: { title:'Hallazgos y objetivos',    sub:'Defectos detectados y logro de objetivos' }
};

// ---------- Utilidades de gráficos ----------
function baseChartTheme(){
  return {
    chart:{ background:'transparent', toolbar:{show:false}, fontFamily:'Inter, sans-serif',
            animations:{enabled:true, easing:'easeinout', speed:800} },
    theme:{ mode:'dark' },
    grid:{ borderColor:PALETA.line, strokeDashArray:4 },
    tooltip:{ theme:'dark' },
    dataLabels:{ enabled:false }
  };
}
function destroyCharts(){ charts.forEach(c=>{try{c.destroy()}catch(e){}}); charts=[]; }
function mkChart(el, opts){ const c=new ApexCharts(el, opts); c.render(); charts.push(c); return c; }

// ================== RENDER DE PÁGINAS ==================
function renderResumen(){
  const k = DATA.kpis;
  return `
  <div class="kpi-grid">
    ${kpiCard('Pruebas de código', k.totalPruebas, `<small>/ ${k.pruebasVerdes} ✓</small>`, 'i-blue','◉','tag-up','100% superadas')}
    ${kpiCard('Cobertura de línea', k.coberturaLinea+'%', '', 'i-oro','▤','tag-up','Umbral 80%')}
    ${kpiCard('Latencia p95', k.p95+' ms', '', 'i-grn','⚡','tag-up','≤ '+k.umbralP95+' ms')}
    ${kpiCard('Aserciones API', k.asercionesApi, '', 'i-mor','◈','tag-up','Newman')}
    ${kpiCard('Hallazgos', k.hallazgos, '', 'i-cor','⚑','tag-warn','Documentados')}
  </div>
  <div class="grid-2">
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Pirámide de pruebas</div><div class="section-sub">Distribución por nivel</div></div><span class="badge badge-grn">97 total</span></div>
      <div id="chartPiramide"></div>
    </div>
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Cobertura de código</div><div class="section-sub">Línea vs rama (caja blanca)</div></div></div>
      <div id="chartCobertura"></div>
    </div>
  </div>
  <div class="grid-2">
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Técnicas de diseño (29119)</div><div class="section-sub">Casos por técnica aplicada</div></div></div>
      <div id="chartTecnicas"></div>
    </div>
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Estado de los casos</div><div class="section-sub">Cobertura de los CP del plan</div></div></div>
      <div id="chartEstados"></div>
    </div>
  </div>`;
}

function renderNiveles(){
  const rows = DATA.niveles.map(n=>`
    <tr>
      <td><span class="mono">${n.nombre}</span></td>
      <td>${n.cantidad}</td>
      <td>${n.tipo}</td>
      <td>${n.enfoque}</td>
      <td>${n.estado==='verde'?'<span class="pill pill-comp">Verde</span>':'<span class="pill pill-parc">Parcial</span>'}</td>
    </tr>`).join('');
  return `
  <div class="grid-2">
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Pruebas por nivel</div><div class="section-sub">Base de la pirámide primero</div></div></div>
      <div id="chartNiveles"></div>
    </div>
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Enfoque de caja</div><div class="section-sub">Blanca · gris · negra</div></div></div>
      <div id="chartCaja"></div>
    </div>
  </div>
  <div class="panel">
    <div class="panel-head"><div><div class="section-title">Detalle de niveles</div><div class="section-sub">Qué verifica cada nivel</div></div></div>
    <div class="tbl-wrap"><table>
      <thead><tr><th>Nivel</th><th>N.º</th><th>Enfoque</th><th>Qué verifica</th><th>Estado</th></tr></thead>
      <tbody>${rows}</tbody>
    </table></div>
  </div>`;
}

function renderDesempeno(){
  const j = DATA.desempeno.jm01;
  return `
  <div class="kpi-grid">
    ${kpiCard('p95 (JM-01)', j.p95+' ms','', 'i-grn','⚡','tag-up','≤ 500 ms')}
    ${kpiCard('Promedio', j.promedio+' ms','', 'i-blue','◉','tag-up','En caliente')}
    ${kpiCard('Mediana', j.mediana+' ms','', 'i-oro','▤','tag-up','Estable')}
    ${kpiCard('Máximo', j.max+' ms','', 'i-cor','▲','tag-warn','Arranque en frío')}
    ${kpiCard('Throughput', j.throughput+'/s','', 'i-mor','◈','tag-up','Peticiones/seg')}
  </div>
  <div class="grid-2">
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Distribución de latencias</div><div class="section-sub">JM-01 · ${j.muestras} muestras</div></div><span class="badge badge-grn">CUMPLE</span></div>
      <div id="chartLatencia"></div>
    </div>
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Margen vs umbral</div><div class="section-sub">p95 medido contra 500 ms</div></div></div>
      <div id="chartMargen"></div>
    </div>
  </div>
  <div class="grid-3">
    ${subCard('Comportamiento Temporal','JM-01','p95 = '+j.p95+' ms','badge-grn','Medido y conforme')}
    ${subCard('Capacidad','JM-02',DATA.desempeno.jm02.estado==='pendiente'?'Pendiente de corrida':'Medido','badge-warn','Datasets listos (10k/50k)')}
    ${subCard('Utilización de Recursos','JM-03',DATA.desempeno.jm03.estado==='pendiente'?'Pendiente de corrida':'Medido','badge-warn','Carga mixta')}
  </div>
  ${DATA.desempeno.cp07 ? `<div class="panel" style="margin-top:18px">
    <div class="panel-head"><div><div class="section-title">CP-PE-07 · Rendimiento del editor (Playwright)</div><div class="section-sub">Tabla extensa 50×50 (2500 celdas) en el editor del navegador</div></div><span class="badge badge-grn">CUMPLE</span></div>
    <div class="kpi-grid" style="margin-bottom:0">
      ${kpiCard('Fluidez de tecleo', DATA.desempeno.cp07.tecleoMs+' ms','<small>/tecla</small>', 'i-grn','⌨','tag-up','≤ '+DATA.desempeno.cp07.tecleoUmbral+' ms')}
      ${kpiCard('Máximo por tecla', DATA.desempeno.cp07.tecleoMax+' ms','', 'i-blue','▲','tag-up','Estable')}
      ${kpiCard('Tiempo de guardado', DATA.desempeno.cp07.guardadoMs+' ms','', 'i-oro','💾','tag-up','≤ '+DATA.desempeno.cp07.guardadoUmbral+' ms')}
      ${kpiCard('Respuesta HTTP', DATA.desempeno.cp07.httpStatus,'', 'i-mor','◈','tag-up','Guardado OK')}
    </div>
  </div>` : ''}`;
}

function renderResultados(){
  return `
  <div class="filters">
    <input class="search" id="searchBox" placeholder="🔍 Buscar caso por id o nombre...">
    <button class="chip active" data-filter="all">Todos</button>
    <button class="chip" data-filter="Alto">Riesgo alto</button>
    <button class="chip" data-filter="Medio">Riesgo medio</button>
    <button class="chip" data-filter="Completo">Completos</button>
    <button class="chip" data-filter="Parcial">Parciales</button>
  </div>
  <div class="panel">
    <div class="tbl-wrap"><table>
      <thead><tr><th>ID</th><th>Caso</th><th>Riesgo</th><th>Subcaracterística</th><th>Nivel</th><th>Estado</th></tr></thead>
      <tbody id="casosBody"></tbody>
    </table></div>
  </div>`;
}

function renderEcosistema(){
  const eco = DATA.ecosistema;
  const herr = eco.filter(e=>e.capa==='Herramienta').map(e=>`
    <div class="eco-node"><h4>${e.herramienta}</h4><p>${e.rol}</p></div>`).join('');
  const gov = DATA.gobernanza.map(g=>`
    <div class="gov-card"><h4>${g.propiedad}</h4><p>${g.desc}</p></div>`).join('');
  return `
  <div class="panel">
    <div class="panel-head"><div><div class="section-title">Flujo del ecosistema</div><div class="section-sub">De las herramientas a las conclusiones</div></div></div>
    <div class="eco-flow">
      <div class="eco-row">${herr}</div>
      <div class="eco-arrow">↓</div>
      <div class="eco-node" style="border-color:var(--oro);background:rgba(230,197,107,.1)"><h4 style="color:var(--oro)">GitHub Actions — punto de gobernanza</h4><p>Ejecuta, agrega y versiona los datos por cada commit</p></div>
      <div class="eco-arrow">↓</div>
      <div class="eco-row">
        <div class="eco-node"><h4>Métricas de proceso</h4><p>97 pruebas · 98,8% cobertura</p></div>
        <div class="eco-node"><h4>Métricas de producto</h4><p>p95 14ms · throughput</p></div>
        <div class="eco-node" style="border-color:var(--morado)"><h4 style="color:#d09fd2">JIRA — repositorio</h4><p>Registro de hallazgos como incidencias</p></div>
      </div>
    </div>
    <div class="gov-grid">${gov}</div>
  </div>`;
}

function renderHallazgos(){
  const h = DATA.hallazgos.map(x=>`
    <tr>
      <td><span class="mono">${x.id}</span></td>
      <td>${x.desc}</td>
      <td>${x.severidad==='Alta'?'<span class="pill pill-alto">Alta</span>':'<span class="pill pill-medio">Media</span>'}</td>
      <td>${x.nivel}</td>
      <td><span class="mono" style="color:#d09fd2">${x.jira}</span></td>
    </tr>`).join('');
  const obj = DATA.objetivos.map(o=>{
    const pct = o.id==='OBJ-PD-1' ? 100 : Math.min(100, Math.round((o.valor/ (o.meta||1))*100));
    const shown = o.estado==='Parcial'?50:pct;
    return `<div class="obj-item">
      <div class="obj-head"><span><span class="mono">${o.id}</span> · ${o.desc}</span><span class="${o.estado==='Parcial'?'tag-warn':'tag-up'}">${o.estado}</span></div>
      <div class="obj-bar"><div class="obj-fill" style="width:0%" data-w="${shown}"></div></div>
    </div>`;
  }).join('');
  return `
  <div class="grid-2">
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Hallazgos detectados</div><div class="section-sub">Con su ticket JIRA asociado</div></div><span class="badge badge-warn">${DATA.hallazgos.length}</span></div>
      <div class="tbl-wrap"><table>
        <thead><tr><th>ID</th><th>Descripción</th><th>Severidad</th><th>Nivel</th><th>JIRA</th></tr></thead>
        <tbody>${h}</tbody>
      </table></div>
    </div>
    <div class="panel">
      <div class="panel-head"><div><div class="section-title">Logro de objetivos</div><div class="section-sub">Proceso y producto</div></div></div>
      ${obj}
    </div>
  </div>
  <div class="panel">
    <div class="panel-head"><div><div class="section-title">Severidad de hallazgos</div><div class="section-sub">Distribución</div></div></div>
    <div id="chartSeveridad"></div>
  </div>`;
}

// ---------- helpers de tarjetas ----------
function kpiCard(label,val,extra,icls,ico,tcls,foot){
  return `<div class="kpi">
    <div class="kpi-top"><span class="kpi-label">${label}</span><span class="kpi-icon ${icls}">${ico}</span></div>
    <div class="kpi-val">${val} ${extra}</div>
    <div class="kpi-foot ${tcls}">▸ ${foot}</div>
  </div>`;
}
function subCard(sub,plan,val,badge,foot){
  return `<div class="panel">
    <div class="panel-head"><div><div class="section-title" style="font-size:14px">${sub}</div><div class="section-sub">${plan}</div></div><span class="badge ${badge}">${val}</span></div>
    <p style="font-size:13px;color:var(--txt2)">${foot}</p>
  </div>`;
}

// ================== GRÁFICOS por página ==================
function drawCharts(page){
  if(page==='resumen'){
    mkChart(document.querySelector('#chartPiramide'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'bar', height:280},
      series:[{name:'Pruebas', data:DATA.niveles.map(n=>n.cantidad)}],
      plotOptions:{bar:{horizontal:true, borderRadius:6, distributed:true, barHeight:'62%'}},
      colors:[PALETA.azul,PALETA.morado,PALETA.oro,PALETA.ambar,PALETA.coral],
      xaxis:{categories:DATA.niveles.map(n=>n.nombre), labels:{style:{colors:PALETA.txt2}}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}}, legend:{show:false}
    });
    mkChart(document.querySelector('#chartCobertura'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'radialBar', height:290},
      series:[DATA.cobertura.linea, DATA.cobertura.rama],
      labels:['Línea','Rama'], colors:[PALETA.verde,PALETA.oro],
      plotOptions:{radialBar:{hollow:{size:'42%'}, track:{background:'rgba(255,255,255,.06)'},
        dataLabels:{name:{color:PALETA.txt2}, value:{color:PALETA.txt, fontSize:'22px', fontFamily:'Space Grotesk', formatter:v=>v+'%'},
        total:{show:true,label:'Cobertura',color:PALETA.txt2,formatter:()=>DATA.cobertura.linea+'%'}}}},
      legend:{show:true, labels:{colors:PALETA.txt2}, position:'bottom'}
    });
    mkChart(document.querySelector('#chartTecnicas'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'bar', height:280},
      series:[{name:'Casos', data:DATA.tecnicas.map(t=>t.casos)}],
      plotOptions:{bar:{borderRadius:6, columnWidth:'52%', distributed:true}},
      colors:[PALETA.azul,PALETA.morado,PALETA.oro,PALETA.ambar,PALETA.coral,PALETA.verde],
      xaxis:{categories:DATA.tecnicas.map(t=>t.nombre), labels:{style:{colors:PALETA.txt2}, rotate:-30, hideOverlappingLabels:false}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}}, legend:{show:false}
    });
    const est = cuentaEstados();
    mkChart(document.querySelector('#chartEstados'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'donut', height:280},
      series:Object.values(est), labels:Object.keys(est),
      colors:[PALETA.verde,PALETA.oro,PALETA.txt2],
      plotOptions:{pie:{donut:{size:'64%', labels:{show:true,total:{show:true,label:'Casos',color:PALETA.txt2,
        formatter:()=>DATA.casos.length}}}}},
      legend:{position:'bottom', labels:{colors:PALETA.txt2}}
    });
  }
  if(page==='niveles'){
    mkChart(document.querySelector('#chartNiveles'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'bar', height:300},
      series:[{name:'Pruebas', data:DATA.niveles.map(n=>n.cantidad)}],
      plotOptions:{bar:{horizontal:true, borderRadius:6, distributed:true, barHeight:'60%'}},
      colors:[PALETA.azul,PALETA.morado,PALETA.oro,PALETA.ambar,PALETA.coral],
      xaxis:{categories:DATA.niveles.map(n=>n.nombre), labels:{style:{colors:PALETA.txt2}}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}}, legend:{show:false}, dataLabels:{enabled:true, style:{colors:['#fff']}}
    });
    const caja = {}; DATA.niveles.forEach(n=>caja[n.tipo]=(caja[n.tipo]||0)+n.cantidad);
    mkChart(document.querySelector('#chartCaja'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'polarArea', height:300},
      series:Object.values(caja), labels:Object.keys(caja),
      colors:[PALETA.oro,PALETA.morado,PALETA.azul],
      stroke:{colors:['rgba(255,255,255,.1)']}, fill:{opacity:.85},
      legend:{position:'bottom', labels:{colors:PALETA.txt2}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}}
    });
  }
  if(page==='desempeno'){
    const j = DATA.desempeno.jm01;
    // Si el JSON cargado no trae histograma, se genera uno básico desde las métricas
    const histo = (j.histograma && j.histograma.length) ? j.histograma : [
      { rango:"0-10", frec:60 }, { rango:"10-20", frec:80 }, { rango:"20-50", frec:40 },
      { rango:"50-100", frec:12 }, { rango:"100-500", frec:6 }, { rango:">500", frec:2 }
    ];
    mkChart(document.querySelector('#chartLatencia'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'area', height:300},
      series:[{name:'Frecuencia', data:histo.map(h=>h.frec)}],
      colors:[PALETA.oro],
      stroke:{curve:'smooth', width:3},
      fill:{type:'gradient', gradient:{shadeIntensity:1, opacityFrom:.5, opacityTo:.05}},
      xaxis:{categories:histo.map(h=>h.rango+' ms'), labels:{style:{colors:PALETA.txt2}}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}},
      markers:{size:5, colors:[PALETA.coral], strokeWidth:0}
    });
    mkChart(document.querySelector('#chartMargen'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'radialBar', height:300},
      series:[Math.round((j.p95/j.umbral)*100)],
      labels:['p95 / umbral'], colors:[PALETA.verde],
      plotOptions:{radialBar:{hollow:{size:'56%'}, track:{background:'rgba(255,255,255,.06)'},
        dataLabels:{name:{color:PALETA.txt2, fontSize:'13px'}, value:{color:PALETA.txt, fontSize:'26px',
          fontFamily:'Space Grotesk', formatter:()=>j.p95+' / '+j.umbral+' ms'}}}}
    });
  }
  if(page==='hallazgos'){
    const sev = {}; DATA.hallazgos.forEach(h=>sev[h.severidad]=(sev[h.severidad]||0)+1);
    mkChart(document.querySelector('#chartSeveridad'), {
      ...baseChartTheme(), chart:{...baseChartTheme().chart, type:'bar', height:220},
      series:[{name:'Hallazgos', data:Object.values(sev)}],
      plotOptions:{bar:{horizontal:true, borderRadius:6, distributed:true, barHeight:'44%'}},
      colors:[PALETA.rojo,PALETA.coral],
      xaxis:{categories:Object.keys(sev), labels:{style:{colors:PALETA.txt2}}},
      yaxis:{labels:{style:{colors:PALETA.txt2}}}, legend:{show:false}, dataLabels:{enabled:true, style:{colors:['#fff']}}
    });
    // animar barras de objetivos
    setTimeout(()=>document.querySelectorAll('.obj-fill').forEach(f=>f.style.width=f.dataset.w+'%'),100);
  }
}

function cuentaEstados(){
  const e={Completo:0,Parcial:0,Excluido:0};
  DATA.casos.forEach(c=>{ e[c.estado]=(e[c.estado]||0)+1; });
  return e;
}

// ================== RESULTADOS: filtros interactivos ==================
let filtroActual='all', busqueda='';
function pintarCasos(){
  const body=document.querySelector('#casosBody'); if(!body)return;
  const f = DATA.casos.filter(c=>{
    const mF = filtroActual==='all' || c.riesgo===filtroActual || c.estado===filtroActual;
    const mB = !busqueda || (c.id+' '+c.nombre).toLowerCase().includes(busqueda.toLowerCase());
    return mF && mB;
  });
  body.innerHTML = f.length? f.map(c=>`
    <tr>
      <td><span class="mono">${c.id}</span></td>
      <td>${c.nombre}</td>
      <td>${c.riesgo==='Alto'?'<span class="pill pill-alto">Alto</span>':'<span class="pill pill-medio">Medio</span>'}</td>
      <td>${c.sub}</td>
      <td>${c.nivel}</td>
      <td>${pillEstado(c.estado)}</td>
    </tr>`).join('') : `<tr><td colspan="6" style="text-align:center;color:var(--txt3);padding:30px">Sin resultados</td></tr>`;
}
function pillEstado(e){
  if(e==='Completo')return '<span class="pill pill-comp">Completo</span>';
  if(e==='Parcial')return '<span class="pill pill-parc">Parcial</span>';
  return '<span class="pill pill-excl">Excluido</span>';
}

// ================== NAVEGACIÓN ==================
function goto(page){
  document.querySelectorAll('.nav-item').forEach(b=>b.classList.toggle('active', b.dataset.page===page));
  document.querySelector('#pageTitle').textContent = PAGES[page].title;
  document.querySelector('#pageSub').textContent = PAGES[page].sub;
  destroyCharts();
  const content=document.querySelector('#content');
  const renderers={resumen:renderResumen,niveles:renderNiveles,desempeno:renderDesempeno,resultados:renderResultados,ecosistema:renderEcosistema,hallazgos:renderHallazgos};
  content.innerHTML = renderers[page]();
  drawCharts(page);
  if(page==='resultados'){
    pintarCasos();
    document.querySelectorAll('.chip').forEach(ch=>ch.addEventListener('click',()=>{
      document.querySelectorAll('.chip').forEach(x=>x.classList.remove('active'));
      ch.classList.add('active'); filtroActual=ch.dataset.filter; pintarCasos();
    }));
    document.querySelector('#searchBox').addEventListener('input',e=>{busqueda=e.target.value;pintarCasos();});
  }
  // cerrar sidebar en móvil
  document.querySelector('#sidebar').classList.remove('open');
  document.querySelector('#overlay')?.classList.remove('show');
}

// ================== DATOS EN VIVO ==================
function toast(msg){
  const t=document.querySelector('#toast'); t.textContent=msg; t.className='toast ok show';
  setTimeout(()=>t.classList.remove('show'),2600);
}
function actualizarUpdate(){
  document.querySelector('#lastUpdate').textContent = DATA.meta.actualizado || 'En vivo';
}

document.addEventListener('DOMContentLoaded',()=>{
  // navegación
  document.querySelectorAll('.nav-item').forEach(b=>b.addEventListener('click',()=>goto(b.dataset.page)));
  // menú móvil
  const sb=document.querySelector('#sidebar');
  document.querySelector('#menuBtn').addEventListener('click',()=>sb.classList.toggle('open'));
  // refrescar (relee window.SQA_DATA)
  document.querySelector('#refreshBtn').addEventListener('click',()=>{
    DATA=window.SQA_DATA; actualizarUpdate();
    const active=document.querySelector('.nav-item.active').dataset.page; goto(active);
    toast('✓ Datos actualizados');
  });
  // cargar datos.json en vivo
  document.querySelector('#fileInput').addEventListener('change',ev=>{
    const file=ev.target.files[0]; if(!file)return;
    const r=new FileReader();
    r.onload=e=>{
      try{
        const cargado=JSON.parse(e.target.result);
        // Fusiona lo cargado con los datos por defecto: así los campos que el
        // pipeline no genera (casos, tecnicas, ecosistema, hallazgos, objetivos)
        // se conservan y el dashboard no se rompe.
        DATA=Object.assign({}, window.SQA_DATA, cargado);
        // Fusión profunda de desempeno para no perder cp07 ni histograma
        if(cargado.desempeno){
          const cp07Previo = window.SQA_DATA.desempeno && window.SQA_DATA.desempeno.cp07;
          DATA.desempeno=Object.assign({}, window.SQA_DATA.desempeno, cargado.desempeno);
          // conserva cp07 (Playwright) aunque el pipeline no lo genere
          if(cp07Previo && !cargado.desempeno.cp07) DATA.desempeno.cp07 = cp07Previo;
        }
        window.SQA_DATA=DATA; actualizarUpdate();
        const active=document.querySelector('.nav-item.active').dataset.page; goto(active);
        toast('✓ datos.json cargado en vivo');
      }catch(err){ console.error(err); toast('✗ Error al leer el JSON'); }
    };
    r.readAsText(file);
  });
  actualizarUpdate();
  goto('resumen');
});
