(() => {
  'use strict';

  const $ = selector => document.querySelector(selector);
  const icons = {
    resumen: ['M3 3h7v7H3z', 'M14 3h7v7h-7z', 'M3 14h7v7H3z', 'M14 14h7v7h-7z'],
    curso: ['M4 4h6c2 0 2 1 2 2v15c0-2-2-3-4-3H3V4z', 'M20 4h-6c-2 0-2 1-2 2v15c0-2 2-3 4-3h5V4z'],
    instructor: ['M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2', 'M13 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0', 'M17 4h5v9h-4', 'M18 8h2'],
    estudiante: ['M2 8l10-5 10 5-10 5z', 'M6 10v7c3 3 9 3 12 0v-7', 'M22 8v7'],
    inscripcion: ['M8 3h8v4H8z', 'M8 5H5v16h14V5h-3', 'M8 14l3 3 5-6'],
    search: ['M19 19l-4-4', 'M17 10a7 7 0 1 1-14 0 7 7 0 0 1 14 0'],
    edit: ['M16 3l5 5-12 12-6 1 1-6z', 'M13 6l5 5'],
    trash: ['M3 6h18', 'M9 6V3h6v3', 'M5 6l1 15h12l1-15', 'M10 10v7', 'M14 10v7'],
    offline: ['M4 4l16 16', 'M3 9c1-1 3-2 4-2', 'M10 5c4-1 8 1 11 4', 'M6 13c2-2 6-3 9-1', 'M10 17l2-1 2 1', 'M12 21h.01']
  };

  const modules = {
    curso: { label: 'Cursos', singular: 'curso', id: 'idCurso', title: 'Un espacio para cada idea.', description: 'Diseña tu oferta académica y conecta cada curso con el instructor ideal.', eyebrow: 'CONOCIMIENTO QUE SE COMPARTE', create: 'Nuevo curso', empty: 'Tu próximo curso empieza aquí', emptyDescription: 'Crea tu primer curso y asígnale uno de tus instructores.', countNote: 'Cursos en tu catálogo', headers: ['Curso', 'Nivel', 'Instructor', 'Acciones'], fields: [{ name: 'titulo', label: 'Título del curso', max: 150, full: true, placeholder: 'Ej. Fundamentos de programación' }, { name: 'descripcion', label: 'Descripción', type: 'textarea', max: 300, full: true, placeholder: 'Describe qué aprenderán los estudiantes…' }, { name: 'nivel', label: 'Nivel', type: 'select', options: ['Básico', 'Intermedio', 'Avanzado'] }, { name: 'idInstructor', label: 'Instructor', type: 'relation', relation: 'instructor' }] },
    instructor: { label: 'Instructores', singular: 'instructor', id: 'idInstructor', title: 'El talento detrás de cada curso.', description: 'Reúne a quienes comparten su experiencia y hacen posible el aprendizaje.', eyebrow: 'PERSONAS QUE INSPIRAN', create: 'Nuevo instructor', empty: 'Conoce a tus próximos instructores', emptyDescription: 'Registra al primer instructor para comenzar a crear tu oferta académica.', countNote: 'Talento en tu comunidad', headers: ['Instructor', 'Especialidad', 'Correo electrónico', 'Acciones'], fields: [{ name: 'nombre', label: 'Nombre completo', max: 100, full: true, placeholder: 'Nombre y apellido' }, { name: 'especialidad', label: 'Especialidad', max: 100, full: true, placeholder: 'Ej. Desarrollo de software' }, { name: 'email', label: 'Correo electrónico', type: 'email', max: 100, full: true, placeholder: 'nombre@ejemplo.com' }] },
    estudiante: { label: 'Estudiantes', singular: 'estudiante', id: 'idEstudiante', title: 'Cada estudiante, una oportunidad.', description: 'Organiza tu comunidad de estudiantes y acompaña su camino de aprendizaje.', eyebrow: 'UNA COMUNIDAD QUE CRECE', create: 'Nuevo estudiante', empty: 'Tu comunidad está por comenzar', emptyDescription: 'Registra al primer estudiante para inscribirlo en tus cursos.', countNote: 'Estudiantes registrados', headers: ['Estudiante', 'Correo electrónico', 'Fecha de nacimiento', 'Acciones'], fields: [{ name: 'nombre', label: 'Nombre completo', max: 100, full: true, placeholder: 'Nombre y apellido' }, { name: 'email', label: 'Correo electrónico', type: 'email', max: 100, full: true, placeholder: 'nombre@ejemplo.com' }, { name: 'fechaNacimiento', label: 'Fecha de nacimiento', type: 'date', full: true }] },
    inscripcion: { label: 'Inscripciones', singular: 'inscripción', id: 'idInscripcion', title: 'Donde comienza el aprendizaje.', description: 'Conecta estudiantes con sus cursos y organiza cada nueva inscripción.', eyebrow: 'OPORTUNIDADES EN MOVIMIENTO', create: 'Nueva inscripción', empty: 'El siguiente paso es aprender', emptyDescription: 'Inscribe a un estudiante en uno de tus cursos. Necesitas ambos registros para comenzar.', countNote: 'Conexiones con el aprendizaje', headers: ['Estudiante', 'Curso', 'Fecha de inscripción', 'Acciones'], fields: [{ name: 'idEstudiante', label: 'Estudiante', type: 'relation', relation: 'estudiante', full: true }, { name: 'idCurso', label: 'Curso', type: 'relation', relation: 'curso', full: true }, { name: 'fechaInscripcion', label: 'Fecha de inscripción', type: 'date', full: true }] }
  };

  const state = { data: Object.fromEntries(Object.keys(modules).map(key => [key, null])), errors: {}, module: 'resumen', loading: true, loadVersion: 0, editing: null, deleting: null, saving: false, deletingBusy: false, toastTimer: null };

  function el(tag, options = {}, ...children) {
    const node = document.createElement(tag);
    for (const [name, value] of Object.entries(options)) {
      if (value === undefined || value === null) continue;
      if (name === 'class') node.className = value;
      else if (name === 'text') node.textContent = String(value);
      else node.setAttribute(name, String(value));
    }
    for (const child of children.flat()) if (child !== null && child !== undefined) node.append(child);
    return node;
  }

  function icon(name) {
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    for (const [key, value] of Object.entries({ viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.6', 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'aria-hidden': 'true', focusable: 'false' })) svg.setAttribute(key, value);
    for (const pathData of icons[name] || icons.curso) {
      const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
      path.setAttribute('d', pathData);
      svg.append(path);
    }
    return svg;
  }

  function initials(value) {
    return String(value || '').trim().split(/\s+/u).filter(Boolean).slice(0, 2).map(word => Array.from(word)[0]).join('').toLocaleUpperCase('es') || '·';
  }
  const normalized = value => String(value ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es');
  const rowsFor = key => state.data[key] || [];
  function related(key, id) { return rowsFor(key).find(row => Number(row[modules[key].id]) === Number(id)); }
  function relatedLabel(key, id) { const row = related(key, id); return row ? row.nombre || row.titulo : `${modules[key].singular[0].toUpperCase()}${modules[key].singular.slice(1)} #${id}`; }
  function dateValue(value) { return String(value || '').slice(0, 10); }
  function today() { const date = new Date(); return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`; }
  function formatDate(value) { const [year, month, day] = dateValue(value).split('-').map(Number); return year && month && day ? new Intl.DateTimeFormat('es-SV', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(year, month - 1, day)) : 'Sin fecha'; }
  function recordLabel(key, row) { return row.nombre || row.titulo || `${relatedLabel('estudiante', row.idEstudiante)} · ${relatedLabel('curso', row.idCurso)}`; }

  async function request(path, options = {}) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 20000);
    try {
      const response = await fetch(path, { ...options, headers: { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...options.headers }, signal: controller.signal, cache: 'no-store' });
      const raw = response.status === 204 ? '' : await response.text();
      let body;
      if (raw) { try { body = JSON.parse(raw); } catch { body = null; } }
      if (!response.ok) {
        const validation = body && typeof body.errors === 'object' && body.errors !== null ? Object.values(body.errors).flat().filter(item => typeof item === 'string').join(' ') : '';
        const detail = validation || body?.detail || body?.message || body?.mensaje || (response.status < 500 ? body?.title : '');
        const messages = { 400: 'Revisa los datos e inténtalo de nuevo.', 404: 'El registro ya no está disponible. Actualiza la página.', 409: 'La operación entra en conflicto con otro registro o una relación existente.', 503: 'La base de datos no está disponible. Verifica que SQL Server esté iniciado.' };
        throw new Error(typeof detail === 'string' && detail ? detail.slice(0, 1800) : messages[response.status] || `No se pudo completar la solicitud (HTTP ${response.status}). Inténtalo de nuevo.`);
      }
      if (raw && body === null) throw new Error('El servidor respondió con un formato inesperado. Vuelve a intentar.');
      return body;
    } catch (error) {
      if (error.name === 'AbortError') throw new Error('La solicitud tardó demasiado. Comprueba que la API y SQL Server estén ejecutándose.');
      if (error instanceof TypeError) throw new Error('No se pudo conectar con la API. Comprueba que el proyecto esté ejecutándose e intenta actualizar.');
      throw error;
    } finally { clearTimeout(timeout); }
  }

  async function loadData() {
    const version = ++state.loadVersion;
    state.loading = true;
    $('#refresh-button').disabled = true;
    $('#refresh-button').setAttribute('aria-label', 'Actualizando registros');
    $('#refresh-icon').classList.add('spinning');
    $('#connection-status').className = 'connection-status';
    $('#connection-status').replaceChildren(el('span', { class: 'status-dot' }), 'Actualizando');
    $('#main').setAttribute('aria-busy', 'true');
    renderStats();
    renderContent();
    const keys = Object.keys(modules);
    const results = await Promise.allSettled(keys.map(async key => {
      const data = await request(`/api/${key}`);
      if (!Array.isArray(data)) throw new Error('No se recibió la lista de registros esperada.');
      return data;
    }));
    if (version !== state.loadVersion) return;
    state.errors = {};
    results.forEach((result, index) => {
      const key = keys[index];
      state.data[key] = result.status === 'fulfilled' ? result.value : null;
      if (result.status === 'rejected') state.errors[key] = result.reason.message;
    });
    state.loading = false;
    $('#refresh-button').disabled = false;
    $('#refresh-button').setAttribute('aria-label', 'Actualizar registros');
    $('#refresh-icon').classList.remove('spinning');
    $('#main').setAttribute('aria-busy', 'false');
    const errors = Object.keys(state.errors);
    $('#connection-status').className = `connection-status ${errors.length ? 'disconnected' : 'connected'}`;
    $('#connection-status').replaceChildren(el('span', { class: 'status-dot' }), errors.length ? 'Conexión incompleta' : 'Datos conectados');
    const alert = $('#global-alert');
    alert.hidden = !errors.length;
    alert.replaceChildren();
    if (errors.length) {
      alert.append(el('strong', { text: `No se pudieron cargar: ${errors.map(key => modules[key].label.toLocaleLowerCase('es')).join(', ')}. ` }), document.createTextNode(state.errors[errors[0]]));
      const retry = el('button', { type: 'button', text: 'Reintentar' });
      retry.addEventListener('click', loadData);
      alert.append(retry);
    }
    renderNavigation();
    renderStats();
    renderContent();
  }

  function renderNavigation() {
    const container = $('#navigation');
    const focusedHref = container.contains(document.activeElement) ? document.activeElement.getAttribute('href') : null;
    container.replaceChildren();
    for (const key of ['resumen', 'curso', 'instructor', 'estudiante', 'inscripcion']) {
      const link = el('a', { class: `nav-item${state.module === key ? ' active' : ''}`, href: `#${key}`, 'aria-current': state.module === key ? 'page' : null }, icon(key), el('span', { text: key === 'resumen' ? 'Resumen' : modules[key].label }));
      if (key !== 'resumen') link.append(el('span', { class: 'nav-count', text: state.data[key] === null ? '—' : state.data[key].length.toLocaleString('es-SV') }));
      container.append(link);
    }
    if (focusedHref) Array.from(container.querySelectorAll('a')).find(link => link.getAttribute('href') === focusedHref)?.focus({ preventScroll: true });
  }

  function renderStats() {
    $('#stats').replaceChildren();
    for (const key of ['curso', 'instructor', 'estudiante', 'inscripcion']) {
      const config = modules[key];
      const value = el('div', { class: 'stat-value' });
      if (state.loading && state.data[key] === null) value.append(el('span', { class: 'stat-loading', 'aria-label': 'Cargando' }));
      else value.textContent = state.data[key] === null ? '—' : state.data[key].length.toLocaleString('es-SV');
      $('#stats').append(el('a', { class: 'stat-card', href: `#${key}`, 'aria-label': `Ver ${config.label.toLocaleLowerCase('es')}` }, el('span', { class: 'stat-label', text: config.label }), value, el('span', { class: 'stat-icon' }, icon(key)), el('div', { class: 'stat-bottom', text: state.errors[key] ? 'Sin conexión' : config.countNote }, el('span', { 'aria-hidden': 'true', text: '↗' }))));
    }
  }

  function renderRoute() {
    const hash = window.location.hash.slice(1);
    state.module = Object.hasOwn(modules, hash) ? hash : 'resumen';
    const overview = state.module === 'resumen';
    const config = modules[state.module];
    document.title = `${overview ? 'Resumen' : config.label} · Aula`;
    $('#breadcrumb-current').textContent = overview ? 'Resumen' : config.label;
    $('#page-title').textContent = overview ? 'Un buen día para aprender.' : config.title;
    $('#page-eyebrow').textContent = overview ? 'TU COMUNIDAD, EN UN VISTAZO' : config.eyebrow;
    $('#page-description').textContent = overview ? 'Organiza tu comunidad académica y haz que cada curso cuente.' : config.description;
    $('#overview-section').hidden = !overview;
    $('#module-section').hidden = overview;
    $('#search-input').value = '';
    $('#level-filter').value = '';
    $('#level-filter').hidden = state.module !== 'curso';
    if (!overview) {
      $('#create-label').textContent = config.create;
      $('#search-input').placeholder = `Buscar ${config.label.toLocaleLowerCase('es')}…`;
    }
    renderNavigation();
    renderContent();
  }

  function loadingState() {
    return el('div', { class: 'loading-lines', role: 'status', 'aria-label': 'Cargando registros' }, [1, 2, 3].map(() => el('div', { class: 'skeleton', 'aria-hidden': 'true' })));
  }

  function emptyState(key, mode = 'empty') {
    const config = modules[key];
    const error = mode === 'error';
    const search = mode === 'search';
    const box = el('div', { class: 'empty-state' }, el('span', { class: 'empty-symbol' }, icon(error ? 'offline' : search ? 'search' : key)), el('h3', { text: error ? 'No pudimos cargar los registros' : search ? 'No encontramos coincidencias' : config.empty }), el('p', { text: error ? 'Revisa la conexión e inténtalo otra vez. Tus datos permanecen en la base de datos.' : search ? 'Prueba con otro término de búsqueda o cambia el filtro de nivel.' : config.emptyDescription }));
    const action = el('button', { class: 'button button-quiet', type: 'button', text: error ? 'Reintentar conexión' : search ? 'Limpiar búsqueda' : config.create });
    action.addEventListener('click', () => {
      if (error) loadData();
      else if (search) { $('#search-input').value = ''; $('#level-filter').value = ''; renderTable(); $('#search-input').focus(); }
      else openEditor(key);
    });
    box.append(action);
    return box;
  }

  function renderContent() {
    if (state.module === 'resumen') renderRecent();
    else renderTable();
  }

  function levelBadge(level) { return el('span', { class: `level-badge ${normalized(level)}`, text: level }); }

  function renderRecent() {
    const container = $('#recent-courses');
    container.replaceChildren();
    if (state.data.curso === null) { container.append(state.loading ? loadingState() : emptyState('curso', 'error')); return; }
    const courses = [...rowsFor('curso')].sort((a, b) => b.idCurso - a.idCurso).slice(0, 3);
    if (!courses.length) { container.append(emptyState('curso')); return; }
    for (const row of courses) {
      const name = row.nombreInstructor || relatedLabel('instructor', row.idInstructor);
      container.append(el('div', { class: 'recent-course' }, el('span', { class: 'course-monogram', 'aria-hidden': 'true', text: initials(row.titulo) }), el('div', { class: 'recent-course-copy' }, el('span', { class: 'recent-course-title', text: row.titulo }), el('span', { class: 'recent-course-meta', text: name })), levelBadge(row.nivel)));
    }
  }

  function searchText(key, row) {
    const values = Object.values(row);
    if (key === 'inscripcion') values.push(relatedLabel('estudiante', row.idEstudiante), relatedLabel('curso', row.idCurso));
    if (key === 'curso') values.push(relatedLabel('instructor', row.idInstructor));
    return normalized(values.join(' '));
  }

  function nameCell(title, subtitle, withInitials = true) {
    const copy = el('div', {}, el('strong', { text: title }), subtitle ? el('span', { class: 'cell-secondary', text: subtitle }) : null);
    return el('td', {}, withInitials ? el('div', { class: 'cell-name' }, el('span', { class: 'cell-initial', 'aria-hidden': 'true', text: initials(title) }), copy) : copy);
  }

  function renderTable() {
    const key = state.module;
    if (!Object.hasOwn(modules, key)) return;
    const config = modules[key];
    const container = $('#table-container');
    container.replaceChildren();
    if (state.data[key] === null) {
      container.append(state.loading ? loadingState() : emptyState(key, 'error'));
      $('#results-label').textContent = state.loading ? 'Cargando registros…' : 'Registros no disponibles';
      return;
    }
    const search = normalized($('#search-input').value.trim());
    const level = key === 'curso' ? $('#level-filter').value : '';
    const all = rowsFor(key);
    const filtered = all.filter(row => (!search || searchText(key, row).includes(search)) && (!level || row.nivel === level)).sort((a, b) => b[config.id] - a[config.id]);
    $('#results-label').textContent = `${filtered.length.toLocaleString('es-SV')} de ${all.length.toLocaleString('es-SV')} ${all.length === 1 ? 'registro' : 'registros'}`;
    if (!filtered.length) { container.append(emptyState(key, search || level ? 'search' : 'empty')); return; }
    const body = el('tbody');
    const table = el('table', { 'aria-label': `Listado de ${config.label.toLocaleLowerCase('es')}` }, el('thead', {}, el('tr', {}, config.headers.map(title => el('th', { scope: 'col', text: title })))), body);
    for (const row of filtered) {
      const tr = el('tr');
      if (key === 'curso') tr.append(nameCell(row.titulo, row.descripcion), el('td', {}, levelBadge(row.nivel)), el('td', { text: row.nombreInstructor || relatedLabel('instructor', row.idInstructor) }));
      if (key === 'instructor') tr.append(nameCell(row.nombre, `Instructor #${row.idInstructor}`), el('td', { text: row.especialidad }), el('td', { text: row.email }));
      if (key === 'estudiante') tr.append(nameCell(row.nombre, `Estudiante #${row.idEstudiante}`), el('td', { text: row.email }), el('td', { text: formatDate(row.fechaNacimiento) }));
      if (key === 'inscripcion') tr.append(nameCell(relatedLabel('estudiante', row.idEstudiante), `Inscripción #${row.idInscripcion}`), nameCell(relatedLabel('curso', row.idCurso), null, false), el('td', { text: formatDate(row.fechaInscripcion) }));
      const actions = el('div', { class: 'row-actions' });
      const label = recordLabel(key, row);
      const edit = el('button', { type: 'button', title: `Editar ${label}`, 'aria-label': `Editar ${label}` }, icon('edit'));
      const remove = el('button', { type: 'button', class: 'delete-button', title: `Eliminar ${label}`, 'aria-label': `Eliminar ${label}` }, icon('trash'));
      edit.addEventListener('click', () => openEditor(key, row));
      remove.addEventListener('click', () => openDelete(key, row));
      actions.append(edit, remove);
      tr.append(el('td', {}, actions));
      body.append(tr);
    }
    container.append(table);
  }

  function openEditor(key, row = null) {
    if (state.saving) return;
    state.editing = { key, row };
    const config = modules[key];
    $('#record-form').reset();
    $('#form-alert').hidden = true;
    $('#form-alert').textContent = '';
    $('#dialog-title').textContent = row ? `Editar ${config.singular}` : config.create;
    $('#dialog-eyebrow').textContent = config.label.toLocaleUpperCase('es');
    $('#dialog-description').textContent = row ? 'Actualiza los datos y guarda los cambios.' : 'Completa la información para incorporar un nuevo registro a tu comunidad.';
    $('#save-record').textContent = row ? 'Guardar cambios' : 'Crear registro';
    $('#save-record').disabled = false;
    const container = $('#form-fields');
    container.replaceChildren();
    const missing = [];
    for (const field of config.fields) {
      const id = `field-${field.name}`;
      const wrapper = el('div', { class: `field${field.full ? ' full-width' : ''}` });
      const label = el('label', { for: id, text: field.label }, el('span', { class: 'required-marker', 'aria-hidden': 'true', text: '*' }));
      let input;
      if (field.type === 'select' || field.type === 'relation') {
        input = el('select', { id, name: field.name, required: '' });
        const placeholder = el('option', { value: '', text: `Selecciona ${field.label.toLocaleLowerCase('es')}` });
        placeholder.disabled = true;
        input.append(placeholder);
        if (field.type === 'select') field.options.forEach(value => input.append(el('option', { value, text: value })));
        else {
          const records = [...rowsFor(field.relation)].sort((a, b) => (a.nombre || a.titulo).localeCompare(b.nombre || b.titulo, 'es'));
          if (!records.length) missing.push(state.data[field.relation] === null ? `No se pudieron cargar los ${modules[field.relation].label.toLocaleLowerCase('es')}. Cierra este formulario y actualiza la conexión.` : `Primero registra al menos un ${modules[field.relation].singular}.`);
          records.forEach(item => input.append(el('option', { value: item[modules[field.relation].id], text: `${item.nombre || item.titulo} · #${item[modules[field.relation].id]}` })));
        }
        input.value = row ? String(row[field.name]) : '';
        if (input.value === '' && row && row[field.name]) {
          const unavailable = el('option', { value: '', text: 'El registro asociado no está disponible', disabled: '' });
          input.append(unavailable);
          input.value = '';
        }
      } else {
        input = el(field.type === 'textarea' ? 'textarea' : 'input', { id, name: field.name, type: field.type === 'textarea' ? null : field.type || 'text', required: '', maxlength: field.max, placeholder: field.placeholder });
        if (field.type === 'date') {
          input.value = row ? dateValue(row[field.name]) : field.name === 'fechaInscripcion' ? today() : '';
          if (field.name === 'fechaNacimiento') input.max = today();
        } else input.value = row ? row[field.name] || '' : '';
      }
      input.addEventListener('input', () => { input.setCustomValidity(''); });
      input.addEventListener('change', () => { input.setCustomValidity(''); });
      wrapper.append(label, input);
      if (field.max && field.type === 'textarea') wrapper.append(el('small', { text: `Hasta ${field.max} caracteres.` }));
      if (field.name === 'idCurso' && key === 'inscripcion') wrapper.append(el('small', { text: 'Un estudiante solo puede inscribirse una vez en el mismo curso.' }));
      container.append(wrapper);
    }
    if (missing.length) {
      $('#form-alert').hidden = false;
      $('#form-alert').textContent = [...new Set(missing)].join(' ');
      $('#save-record').disabled = true;
    }
    $('#record-dialog').showModal();
    setTimeout(() => container.querySelector('input,select,textarea')?.focus(), 0);
  }

  function closeEditor() { if (!state.saving) $('#record-dialog').close(); }
  async function saveRecord(event) {
    event.preventDefault();
    if (state.saving || !state.editing) return;
    const { key, row } = state.editing;
    const config = modules[key];
    const payload = {};
    let valid = true;
    for (const field of config.fields) {
      const input = document.getElementById(`field-${field.name}`);
      const value = input.value.trim();
      input.setCustomValidity(value ? '' : 'Completa este campo.');
      if (!value) valid = false;
      payload[field.name] = field.type === 'relation' ? Number(value) : value;
    }
    if (!valid || !$('#record-form').reportValidity()) { $('#record-form').reportValidity(); return; }
    state.saving = true;
    $('#save-record').disabled = true;
    $('#save-record').textContent = 'Guardando…';
    $('#close-dialog').disabled = true;
    $('#cancel-dialog').disabled = true;
    $('#record-form').setAttribute('aria-busy', 'true');
    $('#form-alert').hidden = true;
    try {
      await request(`/api/${key}${row ? `/${row[config.id]}` : ''}`, { method: row ? 'PUT' : 'POST', body: JSON.stringify(payload) });
      $('#record-dialog').close();
      toast(row ? 'Los cambios se guardaron correctamente.' : 'El registro se creó correctamente.');
      await loadData();
      (state.module === 'resumen' ? $('#hero-create') : $('#create-button')).focus({ preventScroll: true });
    } catch (error) {
      $('#form-alert').textContent = error.message;
      $('#form-alert').hidden = false;
      $('#form-alert').scrollIntoView({ block: 'nearest' });
    } finally {
      state.saving = false;
      $('#save-record').disabled = false;
      $('#save-record').textContent = row ? 'Guardar cambios' : 'Crear registro';
      $('#close-dialog').disabled = false;
      $('#cancel-dialog').disabled = false;
      $('#record-form').setAttribute('aria-busy', 'false');
    }
  }

  function openDelete(key, row) {
    if (state.deletingBusy) return;
    state.deleting = { key, row };
    $('#delete-description').textContent = `Se eliminará ${modules[key].singular === 'inscripción' ? 'la inscripción' : `el ${modules[key].singular}`} «${recordLabel(key, row)}».`;
    $('#delete-alert').hidden = true;
    $('#delete-alert').textContent = '';
    $('#delete-dialog').showModal();
  }

  async function confirmDelete() {
    if (state.deletingBusy || !state.deleting) return;
    const { key, row } = state.deleting;
    state.deletingBusy = true;
    $('#confirm-delete').disabled = true;
    $('#cancel-delete').disabled = true;
    $('#confirm-delete').textContent = 'Eliminando…';
    $('#delete-alert').hidden = true;
    try {
      await request(`/api/${key}/${row[modules[key].id]}`, { method: 'DELETE' });
      $('#delete-dialog').close();
      toast('El registro se eliminó correctamente.');
      await loadData();
      $('#create-button').focus({ preventScroll: true });
    } catch (error) { $('#delete-alert').textContent = error.message; $('#delete-alert').hidden = false; }
    finally { state.deletingBusy = false; $('#confirm-delete').disabled = false; $('#cancel-delete').disabled = false; $('#confirm-delete').textContent = 'Sí, eliminar'; }
  }

  function toast(message) {
    clearTimeout(state.toastTimer);
    $('#toasts').replaceChildren(el('div', { class: 'toast' }, el('span', { class: 'toast-mark', 'aria-hidden': 'true', text: '✓' }), el('span', { text: message })));
    state.toastTimer = setTimeout(() => $('#toasts').replaceChildren(), 6000);
  }

  $('#search-icon').append(icon('search'));
  $('#refresh-button').addEventListener('click', loadData);
  $('#hero-create').addEventListener('click', () => openEditor('curso'));
  $('#create-button').addEventListener('click', () => openEditor(state.module));
  $('#search-input').addEventListener('input', renderTable);
  $('#level-filter').addEventListener('change', renderTable);
  $('#close-dialog').addEventListener('click', closeEditor);
  $('#cancel-dialog').addEventListener('click', closeEditor);
  $('#record-form').addEventListener('submit', saveRecord);
  $('#record-dialog').addEventListener('cancel', event => { if (state.saving) event.preventDefault(); });
  $('#delete-dialog').addEventListener('cancel', event => { if (state.deletingBusy) event.preventDefault(); });
  $('#cancel-delete').addEventListener('click', () => { if (!state.deletingBusy) $('#delete-dialog').close(); });
  $('#confirm-delete').addEventListener('click', confirmDelete);
  window.addEventListener('hashchange', renderRoute);
  document.addEventListener('keydown', event => {
    if (event.key === '/' && !event.ctrlKey && !event.metaKey && !event.altKey && state.module !== 'resumen' && !document.querySelector('dialog[open]') && !['INPUT', 'TEXTAREA', 'SELECT'].includes(document.activeElement.tagName)) {
      event.preventDefault();
      $('#search-input').focus();
    }
  });
  renderNavigation();
  renderRoute();
  loadData();
})();
