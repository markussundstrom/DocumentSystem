using DocumentSystem.Services;

namespace DocumentSystem.Controllers
{
    [Route("api")]
    [ApiController]
    public class DocumentSystemController : ControllerBase {
        private readonly DocumentSystemService m_docserv;

        public DocumentSystemController(DocumentSystemService docserv) {
            this.m_docserv = docserv;
        }

        [HttpGet]
        [Route("tree/{id?}"]
        public async Task<ActionResult<FolderDTO>> GetFolderTree(
                Guid Id = null) {
            FolderDTO tree = m_docserv.ListContents(Id);
            return Ok(tree);
        }
    }
}
