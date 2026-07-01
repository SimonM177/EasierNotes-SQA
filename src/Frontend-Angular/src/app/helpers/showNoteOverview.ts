  export const displayContentOverview = (html: string, maxLength: number): string => {
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, 'text/html');
    const contentOverview = doc.body.textContent || '';
    
    return  contentOverview.length > maxLength ? contentOverview.substring(0, maxLength) + '...' : contentOverview
  };