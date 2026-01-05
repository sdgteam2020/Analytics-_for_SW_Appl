using Domain.interfaces;
using Domain.Requests;
using Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayerEF
{
    public class UnitOfWork : IUnitOfWork
    {
        public IRank Rank { get; }
        public UnitOfWork(IRank rankBL)
        {
            Rank = rankBL;
        }


        public async Task<List<DTOMasterResponse>> GetAllMMaster(DTOMasterRequest Data)
        {
            List<DTOMasterResponse> lst = new List<DTOMasterResponse>();
            if (Data.id == 1)
            {
                var Ret = await Rank.GetAll();
                foreach (var Forma in Ret)
                {

                    DTOMasterResponse db = new DTOMasterResponse();

                    db.Id = Forma.RankId;
                    db.Name = Forma.RankAbbreviation;
                    lst.Add(db);


                }
            }
            return lst;
        }
    }
}
