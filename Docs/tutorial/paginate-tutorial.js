/* Move complete text blocks into measured pages. Fail rather than clip a block. */
async function paginateTutorial(title, sourceName) {
  await document.fonts.ready;
  const source = document.querySelector('#source');
  const blocks = [];
  for (const node of [...source.children]) {
    if (node.matches('table')) {
      // Row-sized units repeat the column headings on each page.
      for (const row of node.querySelectorAll('tbody tr')) {
        const table = node.cloneNode(false);
        table.append(node.querySelector('thead').cloneNode(true));
        const body = document.createElement('tbody');
        body.append(row.cloneNode(true));
        table.append(body);
        blocks.push(table);
      }
    } else if (node.matches('ol,ul')) {
      let number = Number(node.getAttribute('start') || 1);
      for (const item of node.children) {
        const list = node.cloneNode(false);
        if (node.matches('ol')) list.start = number++;
        list.append(item.cloneNode(true));
        blocks.push(list);
      }
    } else blocks.push(node);
  }
  source.remove();
  let content;
  const pages = [];
  function newPage() {
    const page = document.createElement('section');
    page.className = 'page';
    const header = document.createElement('header');
    header.className = 'running-header';
    const brand = document.createElement('span');
    brand.textContent = 'Subscription System Unity';
    const guide = document.createElement('span');
    guide.textContent = title;
    header.append(brand, guide);
    content = document.createElement('main');
    content.className = 'content';
    const footer = document.createElement('footer');
    footer.className = 'page-footer';
    const source = document.createElement('span');
    source.textContent = 'Editable source: Docs/tutorial/' + sourceName;
    footer.append(source, document.createElement('span'));
    page.append(header, content, footer);
    document.body.append(page);
    pages.push(page);
  }
  function fits() {return content.scrollHeight <= content.clientHeight + 1;}
  function place(block) {
    const previous = content.lastElementChild;
    if (block.matches('table') && previous?.matches('table') &&
        block.querySelector('thead').innerHTML === previous.querySelector('thead').innerHTML) {
      const rows = [...block.querySelector('tbody').children];
      previous.querySelector('tbody').append(...rows);
      return () => block.querySelector('tbody').append(...rows);
    }
    if (block.matches('ol,ul') && previous?.tagName === block.tagName &&
        (block.matches('ul') || block.start === previous.start + previous.children.length)) {
      const items = [...block.children];
      previous.append(...items);
      return () => block.append(...items);
    }
    content.append(block);
    return () => block.remove();
  }
  newPage();
  for (let i = 0; i < blocks.length; i++) {
    const block = blocks[i];
    // The short rebuild instructions belong together on the closing page.
    if (block.matches('h2') && /export/i.test(block.textContent) && content.children.length) {
      const undoSection = [];
      for (let j = i; j < blocks.length; j++) {
        if (j > i && blocks[j].matches('h2')) break;
        undoSection.push(place(blocks[j]));
      }
      const sectionOverflows = !fits();
      undoSection.reverse().forEach(undo => undo());
      if (sectionOverflows) newPage();
    }
    const undo = place(block);
    const next = block.matches('h1,h2,h3') ? blocks[i + 1] : null;
    const undoNext = next ? place(next) : null;
    const overflow = !fits();
    if (undoNext) undoNext();
    if (overflow) {
      undo();
      if (!content.children.length) throw Error('Block exceeds page: ' + block.textContent);
      newPage();
      place(block);
      if (!fits()) throw Error('Block exceeds page: ' + block.textContent);
    }
  }
  pages.forEach((page, i) => page.querySelector('footer').lastChild.textContent = `${i + 1} / ${pages.length}`);
  return pages.length;
}
