using Microsoft.AspNetCore.Mvc;
using RpgMvc.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;


namespace RpgMvc.Controllers
{
    public class UsuariosController : Controller
    {
        public string uriBase = "http://lzsouza.somee.com/RpgApi/Usuarios/"; //endereço do somee do professor

        [HttpGet]
        public ActionResult Index()
        {
            return View ("CadastrarUsuario");
        }

        [HttpPost]
        public async Task<ActionResult> RegistrarAsync (UsuarioViewModel u)
        {
            try
            {
                //proximo codigo aqui
                HttpClient httpClient = new HttpClient();
                string uriCompletar = "Registrar";

                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uriBase + uriCompletar, content); // uri base é o local do somee e o completar é o complemento

                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    TempData ["Mensagem"] =
                        string.Format("Usuário {0} Registrado com sucesso! Faça o login para acessar.", u.Username);
                    return View ("AutenticarUsuario");
                }

                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message; //antes do = é uma variavel temporaria do c#
                return RedirectToAction("Index");
            }
        }
        [HttpGet]

        public ActionResult IndexLogin()
        {
            return View("AutenticarUsuario");
        }

        [HttpPost]

        public async Task<ActionResult> AutenticarAsync(UsuarioViewModel u)
        {
            HttpClient httpClient = new HttpClient();
            string uriCompletar = "Autenticar";

            var content = new StringContent(JsonConvert.SerializeObject(u));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.PostAsync(uriBase + uriCompletar, content);

            string serialized = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                UsuarioViewModel uLogado = JsonConvert.DeserializeObject<UsuarioViewModel>(serialized);
                HttpContext.Session.SetString("SessionTokenUsuario", uLogado.Token);
                HttpContext.Session.SetString("SessionUserName", uLogado.Username);
                
                HttpContext.Session.SetString("SessionPerfilUsuario", uLogado.Perfil);
                HttpContext.Session.SetString("SessionIdUsuario", uLogado.Id.ToString());
                
                
                TempData["Mensagem"] = string.Format("Bem-Vindo{0}!!!", uLogado.Username);
                return RedirectToAction("Index", "Personagens");
            }
            else
            {
                throw new System.Exception(serialized);
            }

        }

        [HttpGet]
        public async Task<ActionResult> IndexInformacoesAsync()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                //Novo: Recuperação informação da sessão

                string login = HttpContext.Session.GetString("SessionUsername");

                string uriComplementar =
                $"GetByLogin/{login}";
                HttpResponseMessage response = await httpClient.GetAsync(uriBase +
                uriComplementar);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UsuarioViewModel u = await Task.Run(() =>
                    JsonConvert.DeserializeObject<UsuarioViewModel>(serialized));
                    return View(u);
                }
                else
                {
                    throw new System.Exception(serialized);
                }
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AlterarEmail(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string uriComplementar = "AtualizarEmail";
                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PutAsync(uriBase + uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    TempData["Mensagem"] = "E-mail alterado com sucesso.";
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("IndexInformacoes");
        }

        [HttpGet] 
            public async Task<ActionResult> ObterDadosAlteracaoSenha() 
            { 
                UsuarioViewModel viewModel = new UsuarioViewModel(); 
                try
                { 
                    HttpClient httpClient = new HttpClient(); 
                    string login = HttpContext.Session.GetString("SessionUsername"); 
                    string uriComplementar = $"GetByLogin/{login}"; 
                    HttpResponseMessage response = await httpClient.GetAsync(uriBase + uriComplementar); 
                    
                    string serialized = await response.Content.ReadAsStringAsync(); 
                    
                    TempData["TituloModalExterno"] = "Alteração de Senha"; 
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) 
                    { 
                        viewModel = await Task.Run(() => 
                            JsonConvert.DeserializeObject<UsuarioViewModel>(serialized)); 
                        return PartialView("_AlteracaoSenha", viewModel); 
                    } 
                    else
                        throw new System.Exception(serialized); 
                } 
                catch (System.Exception ex) 
                { 
                    TempData["MensagemErro"] = ex.Message; 
                    return RedirectToAction("IndexInformacoes"); 
                } 
            } 

        [HttpPost]
        public async Task<ActionResult> AlterarSenha(UsuarioViewModel u)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                string token = HttpContext.Session.GetString("SessionTokenUsuario");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                string uriComplementar = "AlterarSenha";
                u.Username = HttpContext.Session.GetString("SessionUsername");
                
                var content = new StringContent(JsonConvert.SerializeObject(u));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PutAsync(uriBase +
                
                uriComplementar, content);
                string serialized = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string mensagem = "Senha alterada com sucesso.";
                    TempData["Mensagem"] = mensagem; //Mensagem guardada do TempData que aparcerá na página pai do modal
                return Json(mensagem); //Mensagem que será exibida no alert da Função que chamou este método
                }
                else
                    throw new System.Exception(serialized);
            }
            catch (System.Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EnviarFoto(UsuarioViewModel u)
        {
            try
            {
                if(Request.Form.Files.Count == 0)
                throw new System.Exception("Selecione o arquivo.");
                
                else
                {
                    var file = Request.Form.Files[0];
                    var fileName = Path.GetFileName(file.FileName);
                    string nomeArquivoSemExtensao = Path.GetFileNameWithoutExtension(fileName);
                    var extensao = Path.GetExtension(fileName);

                    if(extensao != ".jpg" && extensao != ".jpeg" && extensao != ".png")
                        throw new System.Exception("O arquivo selecionado não é uma foto.");

                    //caso precise salvar a imagem em uma pasta do sistema Operacional ou na rede. 
                    //var pastaUpload = @"\" + "";
                    //var path = Path.Combine(pastaUpload, fileName);
                    using(var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        u.Foto = ms.ToArray();
                        //string s = Convert.ToBase64String(fileBytes); //serve para escrever bytes em uma string
                        //System.IO.File.WriteAllBytes(path, ms.ToArray()); //serve para escrever arquivo em uma pasta

                    }
                }
                //trecho referente ao envio ara a API
                HttpClient httpClient = new HttpClient(); 
                    string token = HttpContext.Session.GetString("SessionTokenUsuario"); 
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
                    
                    string uriComplementar = "AtualizarFoto"; 
                    var content = new StringContent(JsonConvert.SerializeObject(u)); 
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); 
 
                    HttpResponseMessage response = await httpClient.PutAsync(uriBase + 
                        uriComplementar, content); 
                    string serialized = await response.Content.ReadAsStringAsync(); 
 
 
                    if (response.StatusCode == System.Net.HttpStatusCode.OK) 
                        TempData["Mensagem"] = "Foto enviada com sucesso"; 
                    
                    else
                        throw new System.Exception(serialized); 

            }
            catch (System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
            }
            return RedirectToAction("IndexInformacoes");
        }

        [HttpGet] 
        public async Task<ActionResult> BaixarFoto() 
        { 
            try
            { 
                HttpClient httpClient = new HttpClient(); 
                string login = HttpContext.Session.GetString("SessionUsername"); 
                string uriComplementar = $"GetByLogin/{login}"; 
                HttpResponseMessage response = await httpClient.GetAsync(uriBase + 
                    uriComplementar); 
 
                string serialized = await response.Content.ReadAsStringAsync(); 
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK) 
                { 
                    UsuarioViewModel viewModel = await 
                    Task.Run(() => JsonConvert.DeserializeObject<UsuarioViewModel>(serialized)); 
 
                //string contentType = "application/image";
                string contentType = System.Net.Mime.MediaTypeNames.Application.Octet; 
                byte[] fileBytes = viewModel.Foto; 
                
                string fileName = $"Foto{viewModel.Username}_{DateTime.Now:ddMMyyyyHHmmss}.png"; // + extensao;
                return File(fileBytes, contentType, fileName); 
                } 
                
                else
                throw new System.Exception(serialized); 
            } 
            
            catch (System.Exception ex) 
            { 
                TempData["MensagemErro"] = ex.Message; 
                return RedirectToAction("IndexInformacoes"); 
            } 
        } 

        [HttpGet]
        public ActionResult Sair()
        {
            try
            {
                HttpContext.Session.Remove("SessionTokenUsuario");
                HttpContext.Session.Remove("SessionUsername");
                HttpContext.Session.Remove("SessionPerfilUsuario");
                HttpContext.Session.Remove("SessionIdUsuario");

                return RedirectToAction("Index", "Home");
            }
            catch(System.Exception ex)
            {
                TempData["MensagemErro"] = ex.Message;
                return RedirectToAction("IndexInformacoes");
            }
        }


    }
}